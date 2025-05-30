using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum QuestTypeEnum
{
    PickUp,
    Elimination,
    Discover,
    Completion,
    Exploration,
    Levelling,
    Experience,
    Standing,
    Loyalty,
    Merchant,
    Skill,
    Multi,
    WeaponAssembly
}
