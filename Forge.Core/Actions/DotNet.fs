namespace Forge.Core.Actions

open System
open System.IO
open System.Text.RegularExpressions
open Faaz
open Forge.Core.Actions
open ToolBox.Core.Processes

[<RequireQualifiedAccess>]
module DotNet =

    let getPublishPath (sc: ScriptContext) = sc.GetValue("publish-dir", "")

    let getTestsPath (sc: ScriptContext) = sc.GetValue("tests-dir", "")

    let getVersion (sc: ScriptContext) = sc.GetValue("version", "")

    let getVersionSuffix (sc: ScriptContext) = sc.TryGetValue("version-suffix")

    let getBuildName (sc: ScriptContext) = sc.GetValue("build-name", "")

    let getSrcPath (sc: ScriptContext) = sc.TryGetValue("src-path")

    let getPackagePath (sc: ScriptContext) = sc.GetValue("package-dir", "")

    let createSourcePath (name: string) (sc: ScriptContext) =
        Path.Combine(getSrcPath sc |> Option.defaultValue "", name)

    let createPackagePath (name: string) (sc: ScriptContext) =
        Path.Combine(getPackagePath sc, $"{name}.{getVersion sc}.nupkg")
        |> fun p -> $"\"{p}\""

    let publish (context: ScriptContext) (dotnetPath: string) name =

        let args =
            [ "publish"
              createSourcePath name context
              "--configuration Release"
              $"--output {Path.Combine(getPublishPath context, name)}"
              // TODO - Add type (such as linux-x64) and version etc(?)
              "-p:UseAppHost=false"
              $"/p:VersionPrefix={getVersion context}"
              match getVersionSuffix context with
              | Some v -> $"/p:VersionSuffix={v}"
              | None -> ""
              $"/p:InformationalVersion={getBuildName context}" ]
            |> (fun a -> String.Join(' ', a))
        //let args =

        //    "publish --configuration Release --output {output} /p:VersionPrefix={}.{}.{}"

        let output, errors = Process.execute dotnetPath args None //(getSrcPath context)

        match errors.Length = 0 with
        | true -> Ok output.Head
        | false -> Error(String.Join(Environment.NewLine, errors))

    /// Run dotnet test and return the past to the results file.
    let test (context: ScriptContext) dotnetPath testName =
        context.Log("dot-net-test", "Running tests.")
        // dotnet test --logger "trx;logfilename=mytests.xml" -r C:\TestResults\FDOM\
        let args =
            [ "test"
              createSourcePath testName context
              $"--logger \"trx;logfilename={testName}.xml\""
              $"-r {getTestsPath context}" ]
            |> (fun a -> String.Join(' ', a))
        //let args =
        //    "publish --configuration Release --output {output} /p:VersionPrefix={}.{}.{}"

        let output, errors = Process.execute dotnetPath args None //path

        match errors.Length = 0 with
        | true ->
            output
            |> List.map (fun o -> context.Log("dot-net-test", o))
            |> ignore

            context.Log("dot-net-test", "Tests complete.")
            Ok(Path.Combine(getTestsPath context, $"{testName}.xml"))
        | false ->
            errors
            |> List.map (fun e -> context.LogError("dot-net-test", e))
            |> ignore

            let errorMessage = String.Join(Environment.NewLine, errors)
            context.LogError("dot-net-test", $"Tests failed. Error: {errorMessage}")
            Error(String.Join(Environment.NewLine, errors))

    /// Run dotnet test and return the past to the results file.

    let pack (context: ScriptContext) (dotnetPath: string) name =

        let args =
            [ "pack"
              createSourcePath name context
              "--configuration Release"
              $"--output {getPackagePath context}"
              $"/p:VersionPrefix={getVersion context}"
              match getVersionSuffix context with
              | Some v -> $"/p:VersionSuffix={v}"
              | None -> ""
              $"/p:InformationalVersion={getBuildName context}" ]
            |> (fun a -> String.Join(' ', a))

        let output, errors = Process.execute dotnetPath args None //(getSrcPath context)

        match errors.Length = 0 with
        | true -> Ok output.Head
        | false -> Error(String.Join(Environment.NewLine, errors))

    let push (context: ScriptContext) (dotnetPath: string) name source =

        let args =
            [ "nuget"
              "push"
              createPackagePath name context
              $"--source \"{source}\"" ]
            |> (fun a -> String.Join(' ', a))


        printfn $"******** Running command: {dotnetPath} {args}"

        let output, errors = Process.execute dotnetPath args None //(getSrcPath context)


        match errors.Length = 0 with
        | true -> Ok output.Head
        | false -> Error(String.Join(Environment.NewLine, errors))


    // TODO move to ToolKit
    module XmlToolKit =

        open System.Xml.Linq

        let xName (name: string) = XName.op_Implicit name

        let tryGetElement (name: string) (parent: XElement) =
            parent.Element name
            |> fun r ->
                match r <> null with
                | true -> Some r
                | false -> None

        let getElements (name: string) (parent: XElement) = parent.Elements name |> List.ofSeq

        let setValue (element: XElement) (value: string) = element.Value <- value


        let tryGetAttribute (name: string) (element: XElement) =
            element.Attribute name
            |> fun r ->
                match r <> null with
                | true -> Some r
                | false -> None

        let load (path: string) =
            File.ReadAllText path |> XDocument.Parse

        let save (path: string) (xDoc: XDocument) = xDoc.Save(path)

    open XmlToolKit

    type PackageVersion = { Name: string }

    type DependencyType =
        | Project
        | Package of PackageVersion

    type Dependency = { Path: string; Type: DependencyType }

    type PackageDependency = { Name: string; Version: string }

    type ProjectDependency = { Name: string; Path: string }

    type ProjectDetails =
        { Name: string
          ProjectFilePath: string
          PackageDependencies: PackageDependency list
          ProjectDependencies: ProjectDependency list }

    let getNameFromProj (path: string) =
        Path.GetFileName path
        |> fun n -> Regex.Replace(n, "(?<fsharp>.fsproj$)|(?<csharp>.csproj$)", "")


    let getDependencies (path: string) =
        // Find the relevant
        let doc = load path

        getElements "ItemGroup" doc.Root
        |> List.map
            (fun ig ->
                let projectDependencies =
                    getElements "ProjectReference" ig
                    |> List.map
                        (fun pr ->
                            match tryGetAttribute "Include" pr with
                            | Some a ->
                                ({ Name = getNameFromProj a.Value
                                   Path = a.Value }: ProjectDependency)
                                |> Ok
                            | None -> Error "Malformed element (missing Include attribute).")

                let packageDependencies =
                    getElements "PackageReference" ig
                    |> List.map
                        (fun pr ->
                            match tryGetAttribute "Include" pr, tryGetAttribute "Version" pr with
                            | Some ia, Some va ->
                                ({ Name = ia.Value; Version = va.Value }: PackageDependency)
                                |> Ok

                            | _ -> Error "Malformed element (missing Include or Version attribute)")

                projectDependencies, packageDependencies)
        |> List.fold
            (fun (accProjs, accPacks) (projs, packs) ->

                let newAccProjs =
                    projs
                    |> List.map
                        (fun r ->
                            match r with
                            | Ok sr -> Some sr
                            | Error _ -> None)
                    |> List.choose id
                    |> fun r -> accProjs @ r

                let newAccPacks =
                    packs
                    |> List.map
                        (fun r ->
                            match r with
                            | Ok sr -> Some sr
                            | Error _ -> None)
                    |> List.choose id
                    |> fun r -> accPacks @ r

                newAccProjs, newAccPacks)
            ([], [])
        |> fun (projs, packs) ->
            ({ Name = getNameFromProj path
               ProjectFilePath = path
               ProjectDependencies = projs
               PackageDependencies = packs }: ProjectDetails)


    let getSolutionDependencies (path: string) =
        let rec searchLoop (dir: string) =

            let di = DirectoryInfo(dir)

            let pds =
                di.EnumerateFiles()
                |> List.ofSeq
                |> List.filter (fun fi -> Regex.IsMatch(fi.Name, ".fsproj$|.csproj$"))
                |> List.map
                    (fun fi ->
                        printfn $"{getNameFromProj fi.Name}"
                        getDependencies fi.FullName)

            let childrenPds =
                di.EnumerateDirectories()
                |> List.ofSeq
                |> List.map (fun di -> searchLoop (di.FullName))
                |> List.concat
                
            pds @ childrenPds

        searchLoop (path)




    let updateProjectFile (context: ScriptContext) (path: string) =

        // Find the relevant
        let doc = load path

        getElements "PropertyGroup" doc.Root
        |> List.map
            (fun pge ->
                // Check if exists.
                match tryGetElement "PackageVersion" pge with
                | Some pv -> setValue pv ""
                | None -> ()

                match tryGetElement "PackageVersion" pge with
                | Some pv -> setValue pv ""
                | None -> ()

                )
        |> ignore



        ()
