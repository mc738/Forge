namespace Forge.Core.Reports

open Fluff.Core
open Forge.Core.Actions


module ProjectReports =
    open Forge.Core.Actions

    let createPackageReferenceObj (packageDependency: DotNet.PackageDependency) =
        Mustache.Value.Object(
            [ "packageName", Mustache.Value.Scalar packageDependency.Name 
              "packageVersion", Mustache.Value.Scalar packageDependency.Version ]
            |> Map.ofList)
    
    let createPackageReferenceObjs (pds: DotNet.PackageDependency list) =
        pds
        |> List.map createPackageReferenceObj
        |> fun r -> Mustache.Value.Array r
        
    
    let createProjectReferenceObj (packageDependency: DotNet.ProjectDependency) =
        Mustache.Value.Object(
            [ "projectName", Mustache.Value.Scalar packageDependency.Name 
              "projectPath", Mustache.Value.Scalar packageDependency.Path ]
            |> Map.ofList)
    
    let rec createProjectReferenceObjs (pds: DotNet.ProjectDependency list) =
        pds
        |> List.map createProjectReferenceObj
        |> fun r -> Mustache.Value.Array r
    
    let generateDotNetProjectReport (template: string) (path: string) =
        DotNet.getSolutionDependencies path
        |> List.map
            (fun pd ->
                Mustache.Value.Object(
                    [ "projectName", Mustache.Value.Scalar pd.Name
                      "packageReferences", createPackageReferenceObjs pd.PackageDependencies
                      "projectReferences", createProjectReferenceObjs pd.ProjectDependencies ]
                    |> Map.ofList))
        |> fun r -> ({ Values = [ "projects", Mustache.Value.Array r ] |> Map.ofList; Partials = Map.empty }: Mustache.Data)
        |> fun v -> Mustache.parse template |> Mustache.replace v true
    
    
    
    


//()
