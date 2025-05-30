using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum DamageEffectType
{
    HeavyBleeding,
    LightBleeding,
    Fracture,
    Contusion,
    Intoxication,
    LethalIntoxication,
    RadExposure,
    Pain,
    DestroyedPart
}
