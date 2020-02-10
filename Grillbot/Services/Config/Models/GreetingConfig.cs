using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Grillbot.Services.Config.Models
{
    public class GreetingConfig
    {
        public string MessageTemplate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GreetingOutputModes OutputMode { get; set; }
    }
}
