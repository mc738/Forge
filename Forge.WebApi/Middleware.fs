namespace Forge.WebApi

open System
open System.Net.WebSockets
open System.Text
open System.Text.Json.Serialization
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http

module Middleware =
    
    [<CLIMutable>]
    type Message ={
        [<JsonPropertyName("text")>]
        Text: string
        
        [<JsonPropertyName("from")>]
        From: string
        
        [<JsonPropertyName("type")>]
        Type: string
        
        [<JsonPropertyName("time")>]
        DateTime: DateTime
    }
    
    module LiveView =
        let mutable sockets = list<WebSocket>.Empty
        
        let addSocket sockets socket = socket :: sockets
        
        let removeSocket sockets socket =
            sockets
            |> List.choose (fun s -> if s <> socket then Some s else printfn "Removing socket"; None)
            
        let private sendMessage =
            fun (socket: WebSocket) (message: string) -> async {
                let buffer = Encoding.UTF8.GetBytes message
                let segment = ArraySegment<byte>(buffer)
                
                if socket.State = WebSocketState.Open then
                    do! socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None)
                        |> Async.AwaitTask
                else
                    sockets <- removeSocket sockets socket
            }
            
        let sendMessageToSockets =
            fun message ->
                async {
                    for socket in sockets do
                        try
                            do! sendMessage socket message
                        with
                            | ex -> printfn $"{socket.State} {ex.Message}"; sockets <- removeSocket sockets socket
                }
                
        let logAction (message: string) =
            (*
            let message =
                    ({ Text = item.Message
                       From = item.From
                       Type = item.ItemType.Serialize()
                       DateTime = item.TimeUtc }: Message)
            *)
            sendMessageToSockets ((*System.Text.Json.JsonSerializer.Serialize*) message)
            |> Async.RunSynchronously
    
    type BuildLogsMiddleware(next: RequestDelegate) =
        member _.Invoke(ctx: HttpContext) =
            async {
                if ctx.Request.Path = PathString("/build_logs/live") then
                    match ctx.WebSockets.IsWebSocketRequest with
                    | true ->
                        use! webSocket =
                            ctx.WebSockets.AcceptWebSocketAsync()
                            |> Async.AwaitTask

                        LiveView.sockets <- LiveView.addSocket LiveView.sockets webSocket
                        printfn $"Socket state: {webSocket.State}"
                        let buffer: byte array = Array.zeroCreate 4096
                        //do! Async.Sleep 5000
                        let! ct = Async.CancellationToken

                        while true do
                            // Needed?
                            do! Async.Sleep 1000

                    | false -> ctx.Response.StatusCode <- 400
                else
                    return! next.Invoke(ctx) |> Async.AwaitTask
            }
            |> Async.StartAsTask
            :> Task
            
    type IApplicationBuilder with
        
        member builder.UseLiveBuildLogs() =
            builder
                .UseWebSockets()
                .UseMiddleware<BuildLogsMiddleware>()