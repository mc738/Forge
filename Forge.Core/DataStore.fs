namespace Forge.Core

open System
open Fipc.Core
open Forge.Core
open Forge.Shared
open Freql.MySql

module DataStore =

    open Forge.Core.Persistence

    let getProject (ctx: MySqlContext) (projectName: string) =
        Operations.selectProjectRecord ctx [ "WHERE name = @0;" ] [ projectName ]


    let getLatestBuild (ctx: MySqlContext) (projectId: int) =
        Operations.selectBuildRecord
            ctx
            [ "WHERE project_id = @0"
              "ORDER BY major DESC, minor DESC, revision DESC"
              "LIMIT 1" ]
            [ projectId ]

    let saveBuild (ctx: MySqlContext) (stats: BuildStats) (projectId: int) =
        ({ ProjectId = projectId
           Reference = Guid.NewGuid()
           Name = stats.Name
           CommitHash = stats.LatestCommitHash
           BuildTime = stats.BuildTime
           Major = stats.Major
           Minor = stats.Minor
           Revision = stats.Revision
           Suffix = stats.VersionSuffix
           BuiltBy = stats.BuiltBy
           Signature = stats.Signature
           Successful = true }: Parameters.NewBuild)
        |> Operations.insertBuild ctx
        |> int

    let saveBuildLog (ctx: MySqlContext) (items: Faaz.Common.LogEntry list) (buildId: int) =
        items
        |> List.map
            (fun li ->
                ({ BuildId = buildId
                   Step = li.Step
                   Entry = li.Entry
                   IsError = li.IsError
                   IsWarning = li.IsWarning }: Parameters.NewBuildLogItem))
        |> fun l -> ctx.InsertList(Records.BuildLogItem.TableName(), l)

    let addDeploymentLocation (ctx: MySqlContext) (name: string) =
        ({ Name = name }: Parameters.NewDeploymentLocation)
        |> Operations.insertDeploymentLocation ctx
        |> int

    let addDeployment (ctx: MySqlContext) (deployment: NewDeployment) =
        let build = Operations.selectBuildRecord ctx [ "WHERE id = @0;" ] [ deployment.BuildId ]
        let location = Operations.selectDeploymentLocationRecord ctx [ "WHERE id = @0;" ] [ deployment.LocationId ]
        
        match build, location with
        | Some build, Some location ->
            ({
                BuildId = build.Id
                LocationId = location.Id
                ArtifactBucket = "forge"
                ArtifactKey = build.Name
                CreatedOn = DateTime.UtcNow
                CompletedOn = None
                Complete = 0uy
                HadErrors = None
                HadWarnings = None
            }: Parameters.NewDeployments)
            |> Operations.insertDeployments ctx
            |> int
            |> Ok
        | None, _ -> Error "Build not found."
        | _, None -> Error "Location not found"
        
    let getDeploymentsForLocation (ctx: MySqlContext) (locationId: int) =
        Operations.selectDeploymentsRecords ctx [ "WHERE location_id = @0 AND complete = FALSE" ] [ locationId ]