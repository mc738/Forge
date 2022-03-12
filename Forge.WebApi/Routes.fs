namespace Forge.WebApi.Routes

open System
//open Microsoft.AspNetCore.Authentication.JwtBearer
open System.Text.Json.Serialization
open Forge.Core
open Forge.Core.Agents
open Forge.Core.Persistence
open Forge.Shared
open Freql.MySql
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Peeps.Monitoring

[<CLIMutable>]
type NewBuild =
    { [<JsonPropertyName("name")>]
      Name: string }


[<CLIMutable>]
type NewSpecific =
    { [<JsonPropertyName("name")>]
      Name: string
      [<JsonPropertyName("major")>]
      Major: int
      [<JsonPropertyName("minor")>]
      Minor: int
      [<JsonPropertyName("revision")>]
      Revision: int }


[<AutoOpen>]
module private Utils =
    let errorHandler (logger: ILogger) name code message =
        logger.LogError("Error '{code}' in route '{name}', message: '{message};.", code, name, message)
        setStatusCode code >=> text message

    //let authorize: (HttpFunc -> HttpContext -> HttpFuncResult) =
    //    requiresAuthentication (challenge JwtBearerDefaults.AuthenticationScheme)

    let getClaim (ctx: HttpContext) (name: string) = ctx.User.FindFirst(name).Value

    let getUserRef (ctx: HttpContext) =
        match Guid.TryParse(getClaim ctx "userRef") with
        | true, ref -> Some(ref)
        | false, _ -> None

    let handleProcResult log name (result: Result<string, string>) next ctx =
        match result with
        | Ok m -> text m next ctx
        | Error e -> errorHandler log name 500 e earlyReturn ctx

module Actions =

    let newMajor: HttpHandler =
        let name = "builds-major"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                let! request = ctx.BindJsonAsync<NewBuild>() |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                let repo = ctx.GetService<BuildAgent>()

                log.LogInformation($"New major build for `{request.Name}` requested.")

                let result = repo.StartMajor(request.Name)
                return text "Build queued." earlyReturn ctx
            }
            |> Async.RunSynchronously

    let newMinor: HttpHandler =
        let name = "builds-minor"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                let! request = ctx.BindJsonAsync<NewBuild>() |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                let repo = ctx.GetService<BuildAgent>()

                log.LogInformation($"New minor build for `{request.Name}` requested.")

                let result = repo.StartMinor(request.Name)
                return text "Build queued." earlyReturn ctx
            }
            |> Async.RunSynchronously


    let newRevision: HttpHandler =
        let name = "builds-revision"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                let! request = ctx.BindJsonAsync<NewBuild>() |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                let repo = ctx.GetService<BuildAgent>()

                log.LogInformation($"New revision build for `{request.Name}` requested.")

                let result = repo.StartRevision(request.Name)
                return text "Build queued." earlyReturn ctx
            }
            |> Async.RunSynchronously


    let newSpecific: HttpHandler =
        let name = "builds-specific"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                let! request =
                    ctx.BindJsonAsync<NewSpecific>()
                    |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                let repo = ctx.GetService<BuildAgent>()

                log.LogInformation($"New revision build for `{request.Name}` requested.")

                let result =
                    repo.StartSpecific(request.Name, request.Major, request.Minor, request.Revision)

                return text "Build queued." earlyReturn ctx
            }
            |> Async.RunSynchronously
            
    let addDeploymentLocation : HttpHandler =
        let name = "add-deployment-location"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                // TODO clean up!
                let! request =
                    ctx.BindJsonAsync<Parameters.NewDeploymentLocation>()
                    |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                log.LogInformation($"Add deployment location request received.")
                let dsCtx = ctx.GetService<MySqlContext>()

                // TODO clean up!
                let id = DataStore.addDeploymentLocation dsCtx request.Name 
                return text $"Deployment location `{request.Name}` added (id: {id})." next ctx
            }
            |> Async.RunSynchronously

    let addDeployment : HttpHandler =
        let name = "add-deployment-location"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                // TODO clean up!
                let! request =
                    ctx.BindJsonAsync<NewDeployment>()
                    |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                log.LogInformation("Add deployment request received.")
                let dsCtx = ctx.GetService<MySqlContext>()

                match DataStore.addDeployment dsCtx request with
                | Ok id -> return text $"Deployment added (id: {id})." next ctx
                | Error e -> return (setStatusCode 404 >=> text e) earlyReturn ctx
            }
            |> Async.RunSynchronously
    
    let getLatestBuild (id: int) : HttpHandler =
        let name = "get-latest-build"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                let! request =
                    ctx.BindJsonAsync<NewSpecific>()
                    |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                log.LogInformation($"Get last build for project `{id}` request received.")
                let dsCtx = ctx.GetService<MySqlContext>()

                match DataStore.getLatestBuild dsCtx id with
                | Some b -> return json b next ctx
                | None -> return (setStatusCode 404 >=> text "Project not found") earlyReturn ctx
            }
            |> Async.RunSynchronously
            
    let getDeploymentsForLocation (id: int) : HttpHandler =
        let name = "get-deployments-for-location"

        fun (next: HttpFunc) (ctx: HttpContext) ->
            async {
                let! request =
                    ctx.BindJsonAsync<NewSpecific>()
                    |> Async.AwaitTask

                let log = ctx.GetLogger(name)

                log.LogInformation($"Get deployments for location `{id}` request received.")
                let dsCtx = ctx.GetService<MySqlContext>()

                return json (DataStore.getDeploymentsForLocation dsCtx id) next ctx
            }
            |> Async.RunSynchronously
            
    let routes: (HttpFunc -> HttpContext -> HttpFuncResult) list =
        [ GET (*Routes.Utils.authorize >=>*)
          >=> choose [ routef "/builds/latest/%i" getLatestBuild
                       routef "/deployments/location/%i" getDeploymentsForLocation ]
          POST
          >=> choose [ route "/actions/build/major" >=> newMajor
                       route "/actions/build/minor" >=> newMinor
                       route "/actions/build/revision" >=> newRevision
                       route "/actions/build/specific" >=> newSpecific
                       route "/actions/deployments/location/add" >=> addDeploymentLocation
                       route "/actions/deployments/add" >=> addDeployment ] ]

module App =

    let routes: (HttpFunc -> HttpContext -> HttpFuncResult) =
        let routes =
            List.concat [ Actions.routes
                          PeepsMetricRoutes.routes ]

        choose routes
