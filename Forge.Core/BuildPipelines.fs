namespace Forge.Core

open System
open System.IO
open System.Text.Json.Serialization
open FStore.S3
open Faaz
open Fipc.Core.Common
open Forge.Core
open Forge.Core.Actions

[<RequireQualifiedAccess>]
module BuildPipeline =

    [<CLIMutable>]
    type Arg =
        { [<JsonPropertyName("key")>]
          Key: string
          [<JsonPropertyName("value")>]
          Value: string }

    type Push =
        { [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("source")>]
          Source: string }

    [<CLIMutable>]
    type Configuration =
        { [<JsonPropertyName("dotnet")>]
          DotNetPath: string
          [<JsonPropertyName("git")>]
          GitPath: string
          [<JsonPropertyName("sourceUrl")>]
          SourceUrl: string
          [<JsonPropertyName("name")>]
          Name: string
          [<JsonPropertyName("tests")>]
          Tests: string list
          [<JsonPropertyName("publishes")>]
          Publishes: string list
          [<JsonPropertyName("packages")>]
          Packages: string list
          [<JsonPropertyName("pushes")>]
          Pushes: Push list
          [<JsonPropertyName("outputDirectory")>]
          OutputDirectory: string
          [<JsonPropertyName("args")>]
          Args: Arg list }

    type Context = { Script: ScriptContext; Stats: BuildStats }

    let getTestPath (sc: ScriptContext) = sc.GetValue("tests-dir", "")
    let getPublishPath (sc: ScriptContext) = sc.GetValue("publish-dir", "")
    let getResourcePath (sc: ScriptContext) = sc.GetValue("resource-dir", "")

    let getSrcDirectory (sc: ScriptContext) = sc.GetValue("src-dir", "")

    let getSrcPath (sc: ScriptContext) = sc.GetValue("src-path", "")

    let getArtifactsPath (sc: ScriptContext) = sc.GetValue("artifacts-dir", "")

    let getBuildArtifactName (bc: Context) =
        Path.Combine(bc.Script.BasePath, $"{bc.Stats.Name}.zip")

    let tmpPath (sc: ScriptContext) = Path.Combine(sc.BasePath, ".tmp")

    let initializeDirectory (config: Configuration) basePath =
        printfn $"Initializing build directory (path: {basePath})."

        let tmpPath = Path.Combine(basePath, ".tmp")

        let testingDir = Path.Combine(tmpPath, "tests")
        let publishDir = Path.Combine(tmpPath, "publish")
        let resourcesDir = Path.Combine(tmpPath, "resources")
        let artifactsDir = Path.Combine(tmpPath, "artifacts")
        let srcDir = Path.Combine(tmpPath, "src")
        let packageDir = Path.Combine(tmpPath, "packages")
        let documentationDir = Path.Combine(tmpPath, "documentation")

        Directory.CreateDirectory(basePath) |> ignore
        Directory.CreateDirectory(tmpPath) |> ignore
        Directory.CreateDirectory(testingDir) |> ignore
        Directory.CreateDirectory(publishDir) |> ignore
        Directory.CreateDirectory(resourcesDir) |> ignore
        Directory.CreateDirectory(artifactsDir) |> ignore
        Directory.CreateDirectory(srcDir) |> ignore
        Directory.CreateDirectory(packageDir) |> ignore

        Directory.CreateDirectory(documentationDir)
        |> ignore

        [ "tests-dir", testingDir
          "publish-dir", publishDir
          "resources-dir", resourcesDir
          "artifacts-dir", artifactsDir
          "src-dir", srcDir
          "src-path", Path.Combine(srcDir, config.Name)
          "package-dir", packageDir
          "document-dir", documentationDir ]

    let createLogger pipeName = Messaging.createClient "build_script" pipeName 

    let createContext (config: Configuration) (id: Guid) basePath paths (logger: FipcConnectionWriter) =
        let args =
            config.Args |> List.map (fun a -> a.Key, a.Value)

        let data =
            List.concat [ args; paths ] |> Map.ofList

        let initStatements = [ BuildStats.TableSql() ]

        ScriptContext.Create(id, config.Name, basePath, data, initStatements, logger)

    let cloneProject (config: Configuration) (srcPath: string) =
        Git.clone config.GitPath config.SourceUrl srcPath

    let createStats (config: Configuration) (srcPath: string) (version: Version) =
        match Git.getLastCommitHash config.GitPath srcPath with
        | Ok lch ->
            let buildTime = DateTime.UtcNow
            let lchSlug = lch.Substring(0, 6)
            let timestamp = buildTime.ToString("yyyyMMddHHmmss")

            let buildName =
                $"{config.Name}-{version.Major}_{version.Minor}_{version.Revision}__{lchSlug}-{timestamp}"

            let signature = "TODO"

            { Reference = Guid.NewGuid()
              Name = buildName
              Major = version.Major
              Minor = version.Minor
              Revision = version.Revision
              VersionSuffix = version.Suffix
              BuildTime = buildTime
              BuiltBy = $"{Environment.MachineName} ({Environment.OSVersion})"
              LatestCommitHash = lch
              Signature = signature }
            |> Ok
        | Error e -> Error e

    let initialize (config: Configuration) (version: Version) (logger: FipcConnectionWriter) : Result<Context, string> =
        let id = Guid.NewGuid()

        printfn "Initializing build context."

        let baseName =
            $"{config.Name}_{version.Major}-{version.Minor}-{version.Revision}"

        let basePath =
            Path.Combine(config.OutputDirectory, baseName)

        let paths = initializeDirectory config basePath

        match createContext config id basePath paths logger with
        | Ok sc ->
            sc.Log("init", "Initialized scripted context.")
            let srcDir = getSrcDirectory sc
            sc.Log("init", $"Cloning project `{config.SourceUrl}` to `{srcDir}`.")

            match cloneProject config srcDir with
            | Ok _ ->
                let srcPath = getSrcPath sc
                sc.Log("init", $"Creating build stats for project `{srcPath}`.")

                match createStats config srcPath version with
                | Ok stats ->
                    let msg =
                        String.Join(
                            Environment.NewLine,
                            [ "Build context initialization complete."
                              "Build stats:"
                              $"\tName: {stats.Name}"
                              $"\tVersion: {stats.GetVersion()}"
                              $"\tLast commit hash: {stats.LatestCommitHash}"
                              $"\tBuild time: {stats.BuildTime}"
                              $"\tBuilt by: {stats.BuiltBy}" ]
                        )

                    let finalSc =
                        sc.AddValue("version", $"{version.Major}.{version.Minor}.{version.Revision}")
                        |> (fun sc -> sc.AddValue("build-name", stats.Name))
                        |> (fun sc ->
                            version.Suffix
                            |> Option.bind (fun vs -> sc.AddValue("version-suffix", vs) |> Some)
                            |> Option.defaultValue sc)
                    //version-suffix

                    finalSc.Writer.Insert("build_stats", stats)
                    // Save all values.

                    finalSc.Log("init", msg)

                    { Script = finalSc; Stats = stats } |> Ok
                | Error e ->
                    sc.Log("init", e)
                    Error e
            | Error e ->
                let m = String.Join(Environment.NewLine, e)
                sc.Log("init", m)
                Error m
        | Error e -> Error e

    let runTest (config: Configuration) name (sc: ScriptContext) =
        sc.Log("run-tests", $"Running `{name}` tests.")
        DotNet.test sc config.DotNetPath name

    let runPublish (config: Configuration) name (sc: ScriptContext) =
        sc.Log("run-publish", $"Publishing `{name}`.")
        DotNet.publish sc config.DotNetPath name

    let runPack (config: Configuration) name (sc: ScriptContext) =
        sc.Log("run-pack", $"Packing `{name}`.")
        DotNet.pack sc config.DotNetPath name

    let runPush (config: Configuration) name source (sc: ScriptContext) =
        sc.Log("run-push", $"Pushing `{name}` to `{source}`.")
        DotNet.push sc config.DotNetPath name source

    let test config (bc: Context) =
        bc.Script.Log("build-pipeline", "Running tests.")

        let testError =
            config.Tests
            |> List.fold
                (fun r t ->
                    match r with
                    | None ->
                        match runTest config t bc.Script with
                        | Ok _ -> None
                        | Error e -> Some e
                    | Some _ -> r)
                None

        match testError with
        | None ->
            bc.Script.Log("build-pipeline", "Tests complete.")
            Ok bc
        | Some e ->
            bc.Script.LogError("build-pipeline", $"Tests error: {e}")
            Error e

    let generateDocumentation config (bc: Context) =
        bc.Script.Log("build-pipeline", "Generating documents")
        match Documentation.generate bc.Script with
        | Ok _ ->
            bc.Script.Log("build-pipeline", "Documents successfully generated.")
            Ok bc
        | Error e ->
            bc.Script.LogError("", "Error generation documents: `{e}`")
            Error e
    
    let publish config (bc: Context) =
        bc.Script.Log("build-pipeline", "Running publish.")

        let publishError =
            config.Publishes
            |> List.fold
                (fun r p ->
                    match r with
                    | None ->
                        match runPublish config p bc.Script with
                        | Ok _ -> None
                        | Error e -> Some e
                    | Some _ -> r)
                None

        match publishError with
        | None ->
            bc.Script.Log("build-pipeline", "Publish complete.")
            Ok bc
        | Some e ->
            bc.Script.LogError("build-pipeline", $"Publish error: {e}")
            Error e

    let pack config (bc: Context) =
        bc.Script.Log("build-pipeline", "Running pack.")

        let publishError =
            config.Packages
            |> List.fold
                (fun r p ->
                    match r with
                    | None ->
                        match runPack config p bc.Script with
                        | Ok _ -> None
                        | Error e -> Some e
                    | Some _ -> r)
                None

        match publishError with
        | None ->
            bc.Script.Log("build-pipeline", "Packing complete.")
            Ok bc
        | Some e ->
            bc.Script.LogError("build-pipeline", $"Packing error: {e}")
            Error e

    let push config (bc: Context) =
        bc.Script.Log("build-pipeline", "Running pushes.")

        let publishError =
            config.Pushes
            |> List.fold
                (fun r p ->
                    match r with
                    | None ->
                        match runPush config p.Name p.Source bc.Script with
                        | Ok _ -> None
                        | Error e -> Some e
                    | Some _ -> r)
                None

        match publishError with
        | None ->
            bc.Script.Log("build-pipeline", "Pushes complete.")
            Ok bc
        | Some e ->
            bc.Script.LogError("build-pipeline", $"Pushes error: {e}")
            Error e

    let createZip (bc: Context) =
        let targetPath = tmpPath bc.Script

        let zipPath =
            Path.Combine(bc.Script.BasePath, $"{bc.Stats.Name}.zip")

        bc.Script.Log("build-pipeline", $"Creating zip `{zipPath}` from `{targetPath}`")

        match attempt (fun _ -> ToolBox.Core.Compression.zip targetPath zipPath) with
        | Ok _ -> Ok bc
        | Error e ->
            bc.Script.LogWarning("build-pipleline", $"Zip failed. Error: {e}")
            Ok bc

    (*
    let createBuildArtifact (bc: Context) =
        let artifactPath = tmpPath bc.Script

        bc.Script.Log("build-pipeline", $"Creating build artifact from `{artifactPath}`")
        match Artifact.create artifactPath bc.Script.BasePath $"{bc.Stats.Name}.build" with
        | Ok path ->
            bc.Script.Log("build-pipeline", $"Artifact `{bc.Stats.Name}.build` created.")
            //bc.Script.Log("build-pipeline", $"Cleaning up build. Deleting `{artifactPath}`.")
            //Directory.Delete(artifactPath, true)
            Ok bc
        | Error e -> Error e
    *)

    let uploadBuildArtifact (bc: Context) =
        match bc.Script.TryGetValue("s3-config-path"), bc.Script.TryGetValue("s3-config-bucket") with
        | Some s3Path, Some bucket ->
            match S3Context.Create(s3Path) with
            | Ok s3 ->
                bc.Script.Log(
                    "build-pipeline",
                    $"S3 context loaded. Uploading to `{s3.Configuration.ServiceUrl}` (Bucket: {bucket})"
                )

                s3.UploadObject(bucket, bc.Stats.Name, getBuildArtifactName bc)
                |> Async.RunSynchronously

                Ok bc
            | Error e ->
                bc.Script.LogError("build-pipeline", $"Error loading s3 context. Path: {s3Path}")
                Error e
        | None, _ -> Error "Missing `s3-config-path` value. Try adding as an arg."
        | _, None -> Error "Missing `s3-config-bucket` value. Try adding as an arg."

    let addTag (config: Configuration) (bc: Context) =
        let version =
            $"v.{bc.Stats.Major}.{bc.Stats.Minor}.{bc.Stats.Revision}"

        bc.Script.Log("build-pipeline", $"Added version tag to git. Tag: {version}")

        match Git.addTag config.GitPath (getSrcPath bc.Script) version with
        | Ok m ->
            match Git.pushTag config.GitPath (getSrcPath bc.Script) version with
            | Ok ok ->
                bc.Script.Log("build-pipline", "Git tag added.")
                Ok bc
            | Error e ->
                printfn $"error {e}"
                Error e
        | Error e ->
            printfn $"error {e}"
            Error e

    let cleanUp (bc: Context) =
        match attempt (fun _ -> Directory.Delete(tmpPath bc.Script, true)) with
        | Ok _ -> Ok bc
        | Error e ->
            bc.Script.LogWarning("build-pipleline", $"Clean up failed. Error: {e}")
            Ok bc

    /// Finish the pipeline and return the build context path.
    let finish (bc: Context) =
        Path.Combine(Path.Combine(bc.Script.BasePath, "context.db"))
        |> Ok
