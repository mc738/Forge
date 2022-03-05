using System.Net.Http.Json;
using Forge.WebApp.Models;
using Microsoft.AspNetCore.Components;

namespace Forge.WebApp.Services;

public class WebApiService
{
    private readonly HttpClient _client;

    private bool _signedIn = false;

    public bool SignedIn => _signedIn;
        
    public WebApiService(HttpClient client)
    {
        _client = client;
    }

    //public async Task<Board?> GetBoard(Guid reference) =>
    //    await _client.GetFromJsonAsync<Board>($"/boards/get?r={reference}");

    public async Task<bool> QueueRevisionBuild(string name)
    {
        var result = await _client.PostAsJsonAsync("/actions/build/revision", new NewBuild() { Name = name});
        return result.IsSuccessStatusCode;
    }
}