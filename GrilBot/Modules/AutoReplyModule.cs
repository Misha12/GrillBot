using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrilBot.Modules
{
    public class AutoReplyModule : IConfigChangeable
    {
        private Dictionary<Regex, string> AutoReplyData { get; set; }

        public AutoReplyModule(IConfigurationRoot configuration)
        {
            Init(configuration);
        }

        private void Init(IConfigurationRoot config)
        {
            var autoReplyData = new Dictionary<Regex, string>();

            foreach (var item in config.GetSection("AutoReply").GetChildren())
            {
                var regexConfig = item.GetSection("IsInMessage");
                var regex = new Regex(regexConfig["Regex"], (RegexOptions)Convert.ToInt32(regexConfig["OptionsFlags"]));

                autoReplyData.Add(regex, item["Reply"]);
            }

            AutoReplyData = autoReplyData;
        }

        public async Task TryReply(SocketUserMessage message)
        {
            var replyMessage = AutoReplyData.FirstOrDefault(o => o.Key.IsMatch(message.Content)).Value;

            if (!string.IsNullOrEmpty(replyMessage))
                await message.Channel.SendMessageAsync(replyMessage);
        }

        public void ConfigChanged(IConfigurationRoot newConfig)
        {
            Init(newConfig);
        }
    }
}
