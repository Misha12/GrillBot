using Newtonsoft.Json;

namespace Grillbot.Models.Config.Dynamic
{
    public class DuckConfig
    {
        [JsonProperty(Required = Required.Always)]
        public string IsKachnaOpenApiBase { get; set; }
        [JsonProperty(Required = Required.Default)]
        public bool EnableBeersOnTap { get; set; } = false;
    }
}