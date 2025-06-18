using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum RequirementState
{
    None,
    Empty,
    TransferItem,
    WorldEvent,
    NotEmpty,
    HasItem,
    WearsItem,
    EmptyOrSize,
    SkillLevel,
    Reference,
    ScavCooperation,
    Train,
    Timer,
    SecretTransferItem
}
