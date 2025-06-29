using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using BodyPartHealth = SPTarkov.Server.Core.Models.Eft.Common.Tables.BodyPartHealth;

namespace SPTarkov.Server.Core.Helpers;

[Injectable]
public class HealthHelper(
    TimeUtil _timeUtil,
    SaveServer _saveServer,
    ProfileHelper _profileHelper,
    ConfigServer _configServer
)
{
    protected readonly HealthConfig _healthConfig = _configServer.GetConfig<HealthConfig>();

    /// <summary>
    ///     Update player profile vitality values with changes from client request object
    /// </summary>
    /// <param name="sessionID">Session id</param>
    /// <param name="pmcProfileToUpdate">Player profile to apply changes to</param>
    /// <param name="healthChanges">Changes to apply </param>
    /// <param name="isDead">OPTIONAL - Is player dead</param>
    public void ApplyHealthChangesToProfile(
        string sessionID,
        PmcData pmcProfileToUpdate,
        BotBaseHealth healthChanges,
        bool isDead = false
    )
    {
        var fullProfile = _saveServer.GetProfile(sessionID);
        var profileEdition = fullProfile.ProfileInfo.Edition;
        var profileSide = fullProfile.CharacterData.PmcData.Info.Side;

        // Get matching 'side' e.g. USEC
        var matchingSide = _profileHelper.GetProfileTemplateForSide(profileEdition, profileSide);

        var defaultTemperature =
            matchingSide?.Character?.Health?.Temperature ?? new CurrentMinMax { Current = 36.6 };

        // Alter saved profiles Health with values from post-raid client data
        ModifyProfileHealthProperties(
            pmcProfileToUpdate,
            healthChanges.BodyParts,
            ["Dehydration", "Exhaustion"]
        );

        // Adjust hydration/energy/temperature
        AdjustProfileHydrationEnergyTemperature(pmcProfileToUpdate, healthChanges);

        // Update last edited timestamp
        pmcProfileToUpdate.Health.UpdateTime = _timeUtil.GetTimeStamp();
    }

    /// <summary>
    ///     Apply Health values to profile
    /// </summary>
    /// <param name="profileToAdjust">Player profile on server</param>
    /// <param name="bodyPartChanges">Changes to apply</param>
    /// <param name="effectsToSkip"></param>
    protected void ModifyProfileHealthProperties(
        PmcData profileToAdjust,
        Dictionary<string, BodyPartHealth> bodyPartChanges,
        HashSet<string>? effectsToSkip = null
    )
    {
        foreach (var (partName, partProperties) in bodyPartChanges)
        {
            if (
                !profileToAdjust.Health.BodyParts.TryGetValue(partName, out var matchingProfilePart)
            )
            {
                continue;
            }

            if (_healthConfig.Save.Health)
            {
                // Apply hp changes to profile
                matchingProfilePart.Health.Current =
                    partProperties.Health.Current == 0
                        ? partProperties.Health.Maximum * _healthConfig.HealthMultipliers.Blacked
                        : partProperties.Health.Current;

                matchingProfilePart.Health.Maximum = partProperties.Health.Maximum;
            }

            // Process each effect for each part
            foreach (var (key, effectDetails) in partProperties.Effects)
            {
                // Null guard
                matchingProfilePart.Effects ??= new Dictionary<string, BodyPartEffectProperties>();

                // Effect already exists on limb in server profile, skip
                if (matchingProfilePart.Effects.ContainsKey(key))
                {
                    // Edge case - effect already exists at destination, but we don't want to overwrite details
                    if (effectsToSkip is not null && effectsToSkip.Contains(key))
                    {
                        matchingProfilePart.Effects[key] = null;
                    }

                    continue;
                }

                if (effectsToSkip is not null && effectsToSkip.Contains(key))
                // Do not pass skipped effect into profile
                {
                    continue;
                }

                var effectToAdd = new BodyPartEffectProperties { Time = effectDetails.Time ?? -1 };
                // Add effect to server profile
                if (matchingProfilePart.Effects.TryAdd(key, effectToAdd))
                {
                    matchingProfilePart.Effects[key] = effectToAdd;
                }
            }
        }
    }

    /// <summary>
    ///     Adjust hydration/energy/temperate
    /// </summary>
    /// <param name="profileToUpdate">Profile to update</param>
    /// <param name="healthChanges"></param>
    protected void AdjustProfileHydrationEnergyTemperature(
        PmcData profileToUpdate,
        BotBaseHealth healthChanges
    )
    {
        // Ensure current hydration/energy/temp are copied over and don't exceed maximum
        var profileHealth = profileToUpdate.Health;
        profileHealth.Hydration.Current =
            profileHealth.Hydration.Current > healthChanges.Hydration.Maximum
                ? healthChanges.Hydration.Maximum
                : Math.Round(healthChanges.Hydration.Current ?? 0);

        profileHealth.Energy.Current =
            profileHealth.Energy.Current > healthChanges.Energy.Maximum
                ? healthChanges.Energy.Maximum
                : Math.Round(healthChanges.Energy.Current ?? 0);

        profileHealth.Temperature.Current =
            profileHealth.Temperature.Current > healthChanges.Temperature.Maximum
                ? healthChanges.Temperature.Maximum
                : Math.Round(healthChanges.Temperature.Current ?? 0);
    }
}
