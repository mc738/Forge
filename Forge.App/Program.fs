open System.IO
open System.Text.RegularExpressions
open Fluff.Core
open Forge.Core.Actions
(*
let template ="""# Solution report
{{#projects}}
## {{projectName}}

Package dependencies:
{{#packageReferences}}
* {{packageName}}
    * Version: `{{packageVersion}}`
{{/packageReferences}}{{^packageReferences}}*No package dependencies*{{/packageReferences}}

Project dependencies:
{{#projectReferences}}
* {{projectName}}
    * Path: `{{projectPath}}`{{/projectReferences}}{{^projectReferences}}*No project dependencies*{{/projectReferences}}
{{/projects}}
"""
*)
//let pds = DotNet.getSolutionDependencies "C:\\Users\\44748\\CommunityBridges"

module Reporting =

    // Raw data
    //  - Commit reports (git)
    //  - Project dependencies (dotnet)

    // Derived data
    //  - Project profile
    //      - Dependent on
    //      - Depends on it
    //      - Files
    //          - "Stability" (i.e. how much it changes).

    type CommitReport = { Name: string; Changes: string list }

    type FileChangeCount =
        { ChangeCounts: Map<string, int> }


        static member Empty() = { ChangeCounts = Map.empty }


    type ProjectGroupSettings = { Name: string; RegexMatch: string }

    type ProjectGroup =
        { Name: string
          Projects: ProjectProfile list }

    and ProjectProfile =
        { Name: string
          Path: string
          References: string list
          ReferencedBy: string list
          ActivityPerCommit: float
          TotalActivity: int
          TotalActivityPercent: float
          ExternalDependencies: ExternalDependency list
          Files: FileProfile list }

    and ExternalDependency = { Name: string; Version: string }

    and FileProfile =
        { Path: string
          ChangeCount: int
          ActivityPerCommitPercent: float
          ProjectActivityPercent: float
          TotalActivityPercent: float
          ChangedIn: string list }

    let getProjectData _ =
        DotNet.getSolutionDependencies "C:\\Users\\44748\\fiket.io\\dotnet"

    let getCommitReports _ =

        match Git.getAllCommits "git" "C:\\Users\\44748\\fiket.io" with
        | Ok commits ->
            let head, tail =
                commits
                |> List.rev
                |> (fun r -> r.Head, r.Tail |> List.rev)

            let headItem =
                match Git.getChangedAllFiles "git" (head.Split(' ').[0]) "C:\\Users\\44748\\fiket.io" with
                | Ok changes ->
                    // printfn $"{}"
                    { Name = head; Changes = changes }
                //|> fun cr -> printfn $"{cr}"
                | Error e -> failwith "Error getting changes for commit"

            tail
            |> List.map
                (fun c ->
                    let commitHash = (c.Split(' ').[0])

                    match Git.getChangedFiles "git" commitHash "C:\\Users\\44748\\fiket.io" with
                    | Ok changes ->
                        printfn $"{c}"
                        { Name = c; Changes = changes }
                    //|> fun cr -> printfn $"{cr}"
                    | Error e -> failwith "Error getting changes for commit")
            //@ [  headItem ]
            |> fun r -> r @ [ headItem ]
        | Error e -> failwith $"Failed to read commits."

    let createFileChangeCounts (reports: CommitReport list) =
        reports
        |> List.fold
            (fun acc cr ->
                let newAcc =
                    cr.Changes
                    |> List.fold
                        (fun acc c ->
                            match acc.ChangeCounts.TryFind c with
                            | Some currentCount ->
                                { acc with
                                      ChangeCounts = acc.ChangeCounts.Add(c, currentCount + 1) }
                            | None ->
                                { acc with
                                      ChangeCounts = acc.ChangeCounts.Add(c, 1) })
                        acc

                newAcc)
            (FileChangeCount.Empty())

    let createFileProfiles (reports: CommitReport list) (fileChangeCount: FileChangeCount) =
        let totalActivity =
            fileChangeCount.ChangeCounts
            |> Map.toList
            |> List.sumBy (fun (_, c) -> c)

        fileChangeCount.ChangeCounts
        |> Map.toList
        |> List.map
            (fun (name, count) ->

                ({ Path = name
                   ChangeCount = count
                   ActivityPerCommitPercent = ((float) count / (float) reports.Length) * 100.
                   ProjectActivityPercent = 0.
                   TotalActivityPercent = ((float) count / (float) totalActivity) * 100.
                   ChangedIn =
                       reports
                       |> List.filter (fun cr -> cr.Changes |> List.contains name)
                       |> List.map (fun cr -> cr.Name) }: FileProfile))

    let getProjectDirectory (fprojPath: string) =
        Path.GetDirectoryName(fprojPath)
        |> fun fp -> fp.Replace('\\', '/')
        |> fun fp -> fp.Replace("C:/Users/44748/fiket.io/", "")

    let matchProject (projectDir: string) (filePath: string) =
        //printfn $"MATCH: {filePath} - {projectDir}"
        Regex.IsMatch(filePath, $"^{projectDir}", RegexOptions.None)

    let projectGroupSettings =
        [ { Name = "Admin"
            RegexMatch = "^dotnet/Fiket.Admin" }
          { Name = "Auth"
            RegexMatch = "^dotnet/Fiket.Auth|^dotnet/Auth" }
          { Name = "Core"
            RegexMatch = "^dotnet/Fiket.Base|^dotnet/Fiket.Utils|^dotnet/Core" }
          { Name = "Demo/test"
            RegexMatch = "^dotnet/Fiket.Test|^dotnet/Fiket.Example" }
          { Name = "Blob store"
            RegexMatch = "^dotnet/Fiket.BlobStore|^dotnet/BlobStore" }
          { Name = "Comms"
            RegexMatch = "^dotnet/Fiket.Comms|^dotnet/Comms" }
          { Name = "DevOps"
            RegexMatch = "^dotnet/Fiket.DevOps" }
          { Name = "Events"
            RegexMatch = "^dotnet/Fiket.Events|^dotnet/Events" }
          { Name = "Routing"
            RegexMatch = "^dotnet/Fiket.Routing" }
          { Name = "Tools"
            RegexMatch = "^dotnet/Fiket.Tools|^dotnet/Tools/Fiket.Tools|^dotnet/Tools/_Examples" }
          { Name = "Workflows"
            RegexMatch = "^dotnet/Fiket.Workflows" }
          { Name = "Workspaces"
            RegexMatch = "^dotnet/Fiket.Workspaces|^dotnet/Workspaces" } ]

    let createGroups (settings: ProjectGroupSettings list) (projects: ProjectProfile list) =
        settings
        |> List.map
            (fun pgs ->
                { Name = pgs.Name
                  Projects =
                      projects
                      |> List.filter (fun p -> Regex.IsMatch(p.Path, pgs.RegexMatch, RegexOptions.None)) })



    let createReport _ =
        let dependencies = getProjectData ()
        let commitReports = getCommitReports ()
        let fileChangeCounts = createFileChangeCounts commitReports

        let fileProfiles =
            createFileProfiles commitReports fileChangeCounts

        let activityCount =
            fileChangeCounts.ChangeCounts
            |> Map.toList
            |> List.sumBy (fun (_, c) -> c)

        dependencies
        |> List.map
            (fun pd ->
                let path =
                    pd.ProjectFilePath |> getProjectDirectory

                let totalActivity, files =
                    fileProfiles
                    |> List.filter (fun fp -> matchProject path fp.Path)
                    |> fun r ->
                        let totalActivity = r |> List.sumBy (fun f -> f.ChangeCount)

                        totalActivity,
                        r
                        |> List.map
                            (fun f ->
                                { f with
                                      ProjectActivityPercent = (float f.ChangeCount / float totalActivity) * 100. })

                ({ Name = pd.Name
                   Path = path
                   References =
                       pd.ProjectDependencies
                       |> List.map (fun p -> p.Name)
                   ReferencedBy =
                       dependencies
                       |> List.filter
                           (fun ipd ->
                               ipd.ProjectDependencies
                               |> List.tryFind (fun p -> p.Name = pd.Name)
                               |> Option.bind (fun _ -> Some true)
                               |> Option.defaultValue false)
                       |> List.map (fun p -> p.Name)
                   ExternalDependencies =
                       pd.PackageDependencies
                       |> List.map (fun ed -> { Name = ed.Name; Version = ed.Version })
                   ActivityPerCommit =
                       (float) totalActivity
                       / (float) commitReports.Length
                   TotalActivity = totalActivity
                   TotalActivityPercent =
                       ((float) totalActivity / (float) activityCount)
                       * 100.
                   Files = files }: ProjectProfile))

    let createReferencesObjs (values: string list) =
        values
        |> List.map
            (fun v ->
                Mustache.Value.Object(
                    [ "referenceLink", Mustache.Value.Scalar "todo"
                      "referenceName", Mustache.Value.Scalar v ]
                    |> Map.ofList
                ))
        |> Mustache.Value.Array

    let createExternalDependenciesObjs (externalDependencies: ExternalDependency list) =
        externalDependencies
        |> List.map
            (fun v ->
                Mustache.Value.Object(
                    [ "externalDependencyName", Mustache.Value.Scalar v.Name
                      "externalDependencyVersion", Mustache.Value.Scalar v.Version ]
                    |> Map.ofList
                )

                )
        |> Mustache.Value.Array

    let createFileProfileObjs (fileProfiles: FileProfile list) =
        fileProfiles
        |> List.map
            (fun fp ->
                Mustache.Value.Object(
                    [ "filePath", Mustache.Value.Scalar fp.Path
                      "fileChanges", Mustache.Value.Scalar <| fp.ChangeCount.ToString()
                      "activityPerCommitPercent",
                      Mustache.Value.Scalar
                      <| fp.ActivityPerCommitPercent.ToString("0.00")
                      "projectActivityPercent",
                      Mustache.Value.Scalar
                      <| fp.ProjectActivityPercent.ToString("0.00")
                      "totalActivityPercent",
                      Mustache.Value.Scalar
                      <| fp.TotalActivityPercent.ToString("0.00")
                      "changedIn",
                      fp.ChangedIn
                      |> List.map
                          (fun ci ->
                              Mustache.Value.Object(
                                  [ "commitName", Mustache.Value.Scalar ci ]
                                  |> Map.ofList
                              ))
                      |> Mustache.Value.Array ]
                    |> Map.ofList
                ))
        |> Mustache.Value.Array

    let createProjectProfileObj (projectProfile: ProjectProfile) =
        Mustache.Value.Object(
            [ "projectName", Mustache.Value.Scalar projectProfile.Name
              "references", createReferencesObjs projectProfile.References
              "referencedIn", createReferencesObjs projectProfile.ReferencedBy
              "externalDepedencies", createExternalDependenciesObjs projectProfile.ExternalDependencies
              "activityPerCommit",
              Mustache.Value.Scalar
              <| projectProfile.ActivityPerCommit.ToString("0.00")
              "totalActivity",
              Mustache.Value.Scalar
              <| projectProfile.TotalActivity.ToString()
              "totalActivityPercent",
              Mustache.Value.Scalar
              <| projectProfile.TotalActivityPercent.ToString("0.00")
              "files", createFileProfileObjs projectProfile.Files ]
            |> Map.ofList
        )

    let toMustacheData (projectGroup: ProjectGroup) =
        Mustache.Value.Object(
            [ "groupName", Mustache.Value.Scalar projectGroup.Name
              "projects",
              Mustache.Value.Array
              <| (projectGroup.Projects
                  |> List.map createProjectProfileObj) ]
            |> Map.ofList
        )

    let generateReport _ =
        let report = createReport ()

        let groups = createGroups projectGroupSettings report

        let template =
            File.ReadAllText "C:\\Users\\44748\\Projects\\__prototypes\\forge\\project_profile.mustache"

        groups
        |> List.map toMustacheData
        |> fun r ->
            ({ Values =
                   [ "projectGroups", Mustache.Value.Array r ]
                   |> Map.ofList
               Partials = Map.empty }: Mustache.Data)
        |> fun v -> Mustache.parse template |> Mustache.replace v true
        |> fun r -> File.WriteAllText("C:\\Users\\44748\\Projects\\__prototypes\\forge\\project_report.html", r)

let report = Reporting.generateReport ()

(*
let template =
    File.ReadAllText "C:\\Users\\44748\\Projects\\__prototypes\\forge\\project_report.mustache"

let report =
    Forge.Core.Reports.ProjectReports.generateDotNetProjectReport template "C:\\Users\\44748\\CommunityBridges"

File.WriteAllText("C:\\Users\\44748\\Projects\\__prototypes\\forge\\project_report.html", report)
*)

//printfn $"{report}"
printfn $"{report}"

// For more information see https://aka.ms/fsharp-console-apps
printfn "Hello from F#"
