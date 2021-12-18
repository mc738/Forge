namespace Forge.Core

open System
open Faaz
open Faaz.ScriptHost
open Faaz.ToolKit.Dev
open Forge.Core.Persistence
open Forge.Core.Persistence.Records
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

        [<RequireQualifiedAccess>]
        type BuildAgentCommand = Build of string * BuildType

        let startBuildAgent (hostContext: HostContext) (context: MySqlContext) (scriptPath: string) =
            MailboxProcessor<BuildAgentCommand>.Start
                (fun inbox ->
                    let rec loop () =
                        async {
                            let! request = inbox.Receive()

                            match request with
                            | BuildAgentCommand.Build (name, buildType) ->

                                let getProjectSql =
                                    [ ProjectRecord.SelectSql()
                                      "WHERE name = @0;" ]
                                    |> String.concat Environment.NewLine

                                printfn "*** Fetching project"
                                match context.SelectSingleAnon<ProjectRecord>(getProjectSql, [ name ]) with
                                | Some project ->

                                    // Get the latest build.
                                    let latestBuildSql =
                                        [ BuildRecord.SelectSql()
                                          "WHERE project_id = @0"
                                          "ORDER BY major DESC, minor DESC, revision DESC"
                                          "LIMIT 1" ]
                                        |> String.concat Environment.NewLine

                                    printfn "*** Fetching latest build details."
                                    let (major, minor, revision) =
                                        match context.SelectSingleAnon<BuildRecord>(latestBuildSql, [ project.Id ]) with
                                        | Some build ->
                                            match buildType with
                                            | BuildType.Major -> build.Major + 1, 0, 0
                                            | BuildType.Minor -> build.Major, build.Minor + 1, 0
                                            | BuildType.Revision -> build.Major, build.Minor, build.Revision + 1
                                        | None -> 0, 1, 0

                                    let command =
                                        $"BuildScripts.{project.Name}.run {major} {minor} {revision}"

                                    match hostContext.Eval<string>(scriptPath, command) with
                                    | Ok path ->

                                        // Connect to the context.db and retrieve data.
                                        let buildCtx = SqliteContext.Open(path)
                                        
                                        let bs = buildCtx.SelectSingle<BuildPipeline.Stats>("build_stats")
                                        let buildLog = buildCtx.Select<Common.LogEntry>("build_logs")
                                        
                                        printfn "*** Saving build."
                                        let buildId =
                                            ({
                                                ProjectId = 1
                                                Reference = Guid.NewGuid()
                                                Name = bs.Name
                                                CommitHash = bs.LatestCommitHash
                                                BuildTime = bs.BuildTime
                                                Major = bs.Major
                                                Minor = bs.Minor
                                                Revision = bs.Revision
                                                Suffix = bs.VersionSuffix
                                                BuiltBy = bs.BuiltBy
                                                Signature = bs.Signature
                                                Successful = true
                                            }: Operations.AddBuildParameters)
                                            |> fun b -> context.Insert(BuildRecord.TableName(), b)
                                            |> int
                                        
                                        printfn "*** Saving build log."    
                                        buildLog
                                        |> List.map (fun li ->
                                            ({
                                            BuildId = buildId
                                            Step = li.Step
                                            Entry = li.Entry
                                            IsError = li.IsError
                                            IsWarning = li.IsWarning
                                        }: Operations.AddBuildLogItemParameters))
                                        |> fun l -> context.InsertList(BuildLogItemRecord.TableName(), l)
                                        
                                        
                                        ()
                                    | Error e -> ()
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
        