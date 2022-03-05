using System.Text.Json.Serialization;

namespace Forge.WebApp.Models;

public class NewBuild
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}