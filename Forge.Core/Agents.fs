namespace Forge.Core

open System
open System.IO
open Faaz
open Faaz.ScriptHost
open Fipc.Core.Common
open Forge.Core.Persistence
open Forge.Core.Persistence
open Freql.MySql
open Freql.Sqlite


module Agents =

    module Internal =

        //type
        [<RequireQualifiedAccess>]
        type BuildType =
            | Major
            | Minor
            | Revision
            | Specific of int * int * int

        [<RequireQualifiedAccess>]
        type BuildAgentCommand = Build of string * BuildType

        let startBuildAgent (hostContext: HostContext) (context: MySqlContext) (scriptsPath: string) =
            
            let pipeName = "build_logs"

            MailboxProcessor<BuildAgentCommand>.Start
                (fun inbox ->
                    let rec loop () =
                        async {
                            let! request = inbox.Receive()

                            match request with
                            | BuildAgentCommand.Build (name, buildType) ->
                                printfn "*** Fetching project"
                                match DataStore.getProject context name with
                                | Some project ->

                                    printfn "*** Fetching latest build details."
                                    let (major, minor, revision) =
                                        match buildType, DataStore.getLatestBuild context project.Id with
                                        //| Some build ->
                                        //match buildType with
                                        | BuildType.Specific (maj, min, rev), _ -> maj, min, rev 
                                        | BuildType.Major, Some build -> build.Major + 1, 0, 0
                                        | BuildType.Minor, Some build -> build.Major, build.Minor + 1, 0
                                        | BuildType.Revision, Some build -> build.Major, build.Minor, build.Revision + 1
                                        | _, None -> 0, 1, 0

                                    // TODO make this config.
                                    let command =
                                        $"{project.ScriptName}.{project.Name}.run \"{pipeName}\" {major} {minor} {revision}"

                                    let scriptPath = Path.Combine(scriptsPath, $"{project.ScriptName}.fsx")
                                    printfn $"Running script `{scriptPath}`."
                                    match hostContext.EvalWithContext(scriptPath,  command) with
                                    | Ok buildCtx ->

                                        // Connect to the context.db and retrieve data.
                                        //let buildCtx = SqliteContext.Open(path)
                                        
                                        let bs = buildCtx.SelectSingle<BuildStats>("build_stats")
                                        let buildLog = buildCtx.Select<Common.LogEntry>("build_logs")
                                        
                                        printfn "*** Saving build."
                                        let buildId = DataStore.saveBuild context bs project.Id
                                        
                                        printfn "*** Saving build log."
                                        DataStore.saveBuildLog context buildLog buildId
                                        printfn "Build complete!"
                                        
                                        ()
                                    | Error e -> printfn $"Error: {e}"
                                | None -> printfn "Project not found."

                            return! loop ()
                        }
                    printfn "Starting build agent."
                    loop ())


    open Internal
    
    type BuildAgent(hostContext: HostContext, context: MySqlContext, scriptPath: string) =
        
        let agent = Internal.startBuildAgent hostContext context scriptPath
        
        member ba.StartMajor(name) = agent.Post(BuildAgentCommand.Build (name, BuildType.Major))
        member ba.StartMinor(name) = agent.Post(BuildAgentCommand.Build (name, BuildType.Minor))
        member ba.StartRevision(name) = agent.Post(BuildAgentCommand.Build (name, BuildType.Revision))
        
        member ba.StartSpecific(name, major, minor, revision) = agent.Post(BuildAgentCommand.Build (name, BuildType.Specific (major, minor, revision)))
