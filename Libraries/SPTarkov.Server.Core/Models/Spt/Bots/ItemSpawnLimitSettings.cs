using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record ItemSpawnLimitSettings
{
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }

    [JsonPropertyName("currentLimits")]
    public Dictionary<MongoId, double>? CurrentLimits { get; set; }

    [JsonPropertyName("globalLimits")]
    public Dictionary<MongoId, double>? GlobalLimits { get; set; }
}
