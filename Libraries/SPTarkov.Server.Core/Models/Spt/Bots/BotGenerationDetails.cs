using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace SPTarkov.Server.Core.Models.Spt.Bots;

public record BotGenerationDetails
{
    /// <summary>
    ///     Should the bot be generated as a PMC
    /// </summary>
    [JsonPropertyName("isPmc")]
    public bool IsPmc { get; set; }

    /// <summary>
    ///     assault/pmcBot etc
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; set; }

    /// <summary>
    ///     assault/pmcBot etc
    /// </summary>
    [JsonPropertyName("BotRoleLowercase")]
    public string RoleLowercase { get; set; }

    /// <summary>
    ///     Side of bot
    /// </summary>
    [JsonPropertyName("side")]
    public string Side { get; set; }

    /// <summary>
    ///     Active players current level
    /// </summary>
    [JsonPropertyName("playerLevel")]
    public int? PlayerLevel { get; set; }

    [JsonPropertyName("playerName")]
    public string? PlayerName { get; set; }

    /// <summary>
    ///     Level specific overrides for PMC level
    /// </summary>
    [JsonPropertyName("locationSpecificPmcLevelOverride")]
    public MinMax<int>? LocationSpecificPmcLevelOverride { get; set; }

    /// <summary>
    ///     Delta of highest level of bot e.g. 50 means 50 levels above player
    /// </summary>
    [JsonPropertyName("botRelativeLevelDeltaMax")]
    public int BotRelativeLevelDeltaMax { get; set; }

    /// <summary>
    ///     Delta of lowest level of bot e.g. 50 means 50 levels below player
    /// </summary>
    [JsonPropertyName("botRelativeLevelDeltaMin")]
    public int BotRelativeLevelDeltaMin { get; set; }

    /// <summary>
    ///     How many to create and store
    /// </summary>
    [JsonPropertyName("botCountToGenerate")]
    public int BotCountToGenerate { get; set; }

    /// <summary>
    ///     Desired difficulty of the bot
    /// </summary>
    [JsonPropertyName("botDifficulty")]
    public string? BotDifficulty { get; set; }

    /// <summary>
    ///     Will the generated bot be a player scav
    /// </summary>
    [JsonPropertyName("isPlayerScav")]
    public bool IsPlayerScav { get; set; }

    [JsonPropertyName("eventRole")]
    public string? EventRole { get; set; }

    [JsonPropertyName("allPmcsHaveSameNameAsPlayer")]
    public bool AllPmcsHaveSameNameAsPlayer { get; set; }

    /// <summary>
    /// Map bots will be spawned on
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// DEFAULT: True
    /// Should the bot container cache be cleared after generating bot equipment + loot
    /// </summary>
    [JsonPropertyName("clearBotContainerCacheAfterGeneration")]
    public bool ClearBotContainerCacheAfterGeneration { get; set; } = true;

    /// <summary>
    /// Level the bot will have once generated
    /// </summary>
    public int BotLevel { get; set; }

    /// <summary>
    /// Version of the game bot will use - Only apples to PMCs
    /// </summary>
    public string GameVersion { get; set; }
}
