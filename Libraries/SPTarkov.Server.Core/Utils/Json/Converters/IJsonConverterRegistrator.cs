using System.Text.Json.Serialization;

namespace SPTarkov.Server.Core.Utils.Json.Converters;

public interface IJsonConverterRegistrator
{
    public IEnumerable<JsonConverter> GetJsonConverters();
}
