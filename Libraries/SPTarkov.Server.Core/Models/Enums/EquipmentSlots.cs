using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
[EftListEnumConverter]
public enum EquipmentSlots
{
    Headwear,
    Earpiece,
    FaceCover,
    ArmorVest,
    Eyewear,
    ArmBand,
    TacticalVest,
    Pockets,
    Backpack,
    SecuredContainer,
    FirstPrimaryWeapon,
    SecondPrimaryWeapon,
    Holster,
    Scabbard,
}
