using Grillbot.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Grillbot.Models.Config.Dynamic
{
    public class GreetingConfig
    {
        public string MessageTemplate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GreetingOutputModes OutputMode { get; set; }
    }
}
