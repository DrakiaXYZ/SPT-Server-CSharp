using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum ArmorMaterial
{
    UHMWPE,
    Aramid,
    Combined,
    Titan,
    Aluminium,
    ArmoredSteel,
    Ceramic,
    Glass
}
