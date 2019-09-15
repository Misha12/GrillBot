using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Grillbot.Services.Config.Models
{
    public class GreetingConfig : MethodConfigBase
    {
        public string MessageTemplate { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public GreetingOutputModes OutputMode { get; set; }
    }
}
