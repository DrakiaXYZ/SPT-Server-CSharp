using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum SeasonalEventType
{
    None,
    Christmas,
    Halloween,
    NewYears,
    Promo,
    AprilFools
}
