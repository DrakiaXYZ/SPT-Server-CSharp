using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum ExfiltrationType
{
    Individual,
    SharedTimer,
    Manual
}
