using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum ThrowWeapType
{
    frag_grenade,
    flash_grenade,
    stun_grenade,
    smoke_grenade,
    gas_grenade,
    incendiary_grenade,
    sonar,
}
