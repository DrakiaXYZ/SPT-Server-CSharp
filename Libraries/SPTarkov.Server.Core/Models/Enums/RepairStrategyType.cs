using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum RepairStrategyType
{
    MeleeWeapon,
    Firearms,
    Armor
}
