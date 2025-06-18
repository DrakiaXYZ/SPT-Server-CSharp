using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum QuestStatusEnum
{
    Locked = 0,
    AvailableForStart = 1,
    Started = 2,
    AvailableForFinish = 3,
    Success = 4,
    Fail = 5,
    FailRestartable = 6,
    MarkedAsFailed = 7,
    Expired = 8,
    AvailableAfter = 9,
}
