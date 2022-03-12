using System.Text.Json.Serialization;

namespace Forge.Shared;

public class NewDeployment
{
    [JsonPropertyName("buildId")] public int BuildId { get; set; }
    [JsonPropertyName("buildId")] public int LocationId { get; set; }
}