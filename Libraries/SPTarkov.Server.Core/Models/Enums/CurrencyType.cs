using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum CurrencyType
{
    RUB,
    USD,
    EUR,
    GP
}
