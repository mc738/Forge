open System.IO
open ToolBox.Core
open ToolBox.Core.Compression
open FStore.S3
open Forge.Core.Actions

module Utils =

    let clearDirectory path =
        DirectoryInfo(path)
        |> fun di ->
            di.GetDirectories()
            |> Array.iter (fun cdi -> Directory.Delete(cdi.FullName, true))

            di.GetFiles()
            |> Array.iter (fun fi -> File.Delete(fi.FullName))

    let moveDirectoryContains (path: string) (outputPath: string) =
        DirectoryInfo(path)
        |> fun di ->
            di.GetDirectories()
            |> Array.iter (fun cdi -> Directory.Move(cdi.FullName, Path.Combine(outputPath, cdi.Name)))

            di.GetFiles()
            |> Array.iter (fun fi -> File.Move(fi.FullName, Path.Combine(outputPath, fi.Name)))

    let stopService (name: string) =
        Processes.Process.run (
            { Name = ""
              Args = ""
              StartDirectory = None }: Processes.Process.ProcessParameters
        )

    let startService (name: string) =
        Processes.Process.run (
            { Name = ""
              Args = ""
              StartDirectory = None }: Processes.Process.ProcessParameters
        )

module Artifacts =

    // Test - ToolBox-0_2_0__761e9a-20220226132713

    let fetch (s3Ctx: S3Context) (bucket: string) (key: string) (savePath: string) =
        S3.download s3Ctx bucket key savePath

    let unpack (path: string) (outputPath: string) = unzip path outputPath

let dataPath = "C:\\ProjectData\\Forge\\deployment"
let name = "ToolBox-0_2_0__761e9a-20220226132713"

match
    S3Context.Create
    <| Path.Combine(dataPath, "s3_config.json")
    with
| Ok s3Ctx ->
    let artifactPath =
        Path.Combine(dataPath, "artifacts", name)

    let tempDir =
        $"C:\\ProjectData\\Forge\deployment\\.tmp\\{name}"

    Directory.CreateDirectory(tempDir) |> ignore

    let savePath =
        "C:\\ProjectData\\test_deployments\\ToolBox"

    printfn $"Clearing output directory {savePath}..."
    Utils.clearDirectory savePath

    if Directory.Exists tempDir then
        Directory.Delete(tempDir, true)

    ConsoleIO.printSuccess "Complete"

    printfn $"Fetching artifact {name}..."
    // Fetch artifact.
    match File.Exists artifactPath with
    | true -> ConsoleIO.printWarning "Artifact already exists. Skipping fetch."
    | false ->
        printfn $"Fetching from S3"

        match Artifacts.fetch s3Ctx "forge" name artifactPath with
        | Ok _ ->
            ConsoleIO.printSuccess $"Complete"
            printfn "Waiting for save file handle..."
            Async.Sleep 5000 |> Async.RunSynchronously
        | Error e ->
            ConsoleIO.printError $"Error fetching artifact: {e}"
            failwith e



    printfn $"Unzipping to path `{tempDir}`..."
    Artifacts.unpack artifactPath tempDir
    ConsoleIO.printSuccess "Complete"

    // Extract contains.
    printfn $"Extracting source to output `{savePath}`..."
    let p = Path.Combine(tempDir, "src", "ToolBox")
    Utils.moveDirectoryContains p savePath
    ConsoleIO.printSuccess "Complete"

    printfn "Cleaning up..."
    Directory.Delete(tempDir, true)
    ConsoleIO.printSuccess "Complete"

    (*
    Artifacts.fetch s3Ctx "forge" "ToolBox-0_2_0__761e9a-20220226132713" tempPath
    |> Result.bind (fun _ -> Async.Sleep 5000 |> Async.RunSynchronously; Artifacts.unpack tempPath savePath |> Ok)
    *)

    ()
| Error e -> ConsoleIO.printError $"Error loading s3 context: {e}"

// Check for actions.

// Fetch artifact.

// Unpack.

// Stop service.

// Deploy.

// Start service.

// Report results.



// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
