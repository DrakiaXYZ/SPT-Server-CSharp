using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models;

[EftEnumConverter]
public enum RadioStationType
{
    None,
    Christmas,
    RunddansEvent,
    HipHop,
    Acoustic,
    EDM,
    Rock,
    LoFi,
    Metal,
    Punk,
    Pop
}
