using System;
using Grillbot.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Grillbot.Models.Duck
{
    public class CurrentState
    {
        [JsonProperty(Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DuckState State { get; set; }

        public DateTime LastChange { get; set; }
        public DateTime? ExpectedEnd { get; set; }
        public string[] BeersOnTap { get; set; }
        public string OpenedByName { get; set; }
        public string OpenedByDiscordGlobalNick { get; set; }
        public string EventName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DuckState NextPlannedState { get; set; }

        public DateTime? NextStateDateTime { get; set; }
        public DateTime? NextOpeningDateTime { get; set; }

        public string Note { get; set; }
    }
}