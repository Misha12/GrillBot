using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatchDog_Bot.Services.Statistics;

namespace WatchDog_Bot.Modules
{
    public class AutoReplyModule : IConfigChangeable
    {
        private Dictionary<string, string> AutoReplyData { get; }

        public AutoReplyModule(IConfigurationRoot configuration)
        {
            AutoReplyData = new Dictionary<string, string>();
            Init(configuration);
        }

        private void Init(IConfigurationRoot config)
        {
            foreach (var item in config.GetSection("AutoReply").GetChildren())
            {
                AutoReplyData.Add(item["WhenSay"], item["Reply"]);
            }
        }

        public async Task TryReply(SocketUserMessage message)
        {
            var replyMessage = AutoReplyData.FirstOrDefault(o =>
                message.Content.Contains(o.Key, StringComparison.InvariantCultureIgnoreCase)).Value;

            if(!string.IsNullOrEmpty(replyMessage))
                await message.Channel.SendMessageAsync(replyMessage);
        }

        public void ConfigChanged(IConfigurationRoot newConfig)
        {
            AutoReplyData.Clear();
            Init(newConfig);
        }
    }
}
