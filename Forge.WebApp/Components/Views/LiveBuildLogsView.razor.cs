using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace Forge.WebApp.Components.Views;

public class LiveBuildLogsViewBase: ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _disposalTokenSource = new CancellationTokenSource();
    private readonly ClientWebSocket _webSocket = new ClientWebSocket();
    protected string Message = "Hello, websocket!";
    protected List<string> Log = new();

    protected override async Task OnInitializedAsync()
    {
        await _webSocket.ConnectAsync(new Uri("ws://localhost:11115/build_logs/live"), _disposalTokenSource.Token);
        Console.WriteLine("Connected to ws");
        _ = ReceiveLoop();
    }
        
    async Task ReceiveLoop()
    {
        var buffer = new ArraySegment<byte>(new byte[1024]);
        while (!_disposalTokenSource.IsCancellationRequested)
        {
            // Note that the received block might only be part of a larger message. If this applies in your scenario,
            // check the received.EndOfMessage and consider buffering the blocks until that property is true.
            // Or use a higher-level library such as SignalR.
            var received = await _webSocket.ReceiveAsync(buffer, _disposalTokenSource.Token);
            var receivedAsText = Encoding.UTF8.GetString(buffer.Array, 0, received.Count);
            Log.Add(receivedAsText);
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        _disposalTokenSource.Cancel();
        _ = _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
    }
}