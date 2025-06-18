using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum TraderServiceType
{
    ExUsecLoyalty,
    ZryachiyAid,
    CultistsAid,
    BtrItemsDelivery,
    PlayerTaxi,
    BtrBotCover,
    TransitItemsDelivery,
}
