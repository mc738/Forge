namespace Forge.WebApi.Routes

open System
//open Microsoft.AspNetCore.Authentication.JwtBearer
open System.Text.Json.Serialization
open Forge.Core.Agents
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Giraffe
open Peeps.Monitoring

[<CLIMutable>]
type NewBuild = { [<JsonPropertyName("name")>] Name: string }


[<CLIMutable>]
type NewSpecific = {
    [<JsonPropertyName("name")>] Name: string
    [<JsonPropertyName("major")>] Major: int
    [<JsonPropertyName("minor")>] Minor: int
    [<JsonPropertyName("revision")>] Revision: int
}


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
                let! request = ctx.BindJsonAsync<NewSpecific>() |> Async.AwaitTask

                let log = ctx.GetLogger(name)
            
                let repo = ctx.GetService<BuildAgent>()

                log.LogInformation($"New revision build for `{request.Name}` requested.")

                let result = repo.StartSpecific(request.Name, request.Major, request.Minor, request.Revision)
                return text "Build queued." earlyReturn ctx
            }
            |> Async.RunSynchronously
    
    let routes: (HttpFunc -> HttpContext -> HttpFuncResult) list =
         [ POST
                       >=> choose [ route "/actions/build/major" >=> newMajor
                                    route "/actions/build/minor" >=> newMinor
                                    route "/actions/build/revision" >=> newRevision
                                    route "/actions/build/specific" >=> newSpecific  ]
                       (*
                       GET
                       >=> choose [ route "/services/admin/overviews"
                                    >=> warbler (fun _ -> getServiceOverviews)
                                    route "/services/admin/details"
                                    >=> warbler (fun _ -> getServiceDetails) ]*) ]



module App =
    
    
    let routes: (HttpFunc -> HttpContext -> HttpFuncResult) =
        let routes =
            List.concat [
                Actions.routes
                PeepsMetricRoutes.routes
            ]

        choose routes

