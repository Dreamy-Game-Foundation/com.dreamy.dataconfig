using Newtonsoft.Json;

namespace Dreamy.DataConfig
{
    public abstract class DataConfigRow
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }
    }
}
