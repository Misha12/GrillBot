using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public class AutoReplyService : IConfigChangeable
    {
        private Dictionary<Regex, string> AutoReplyData { get; set; }
        private Dictionary<string, uint> Stats { get; set; }

        public AutoReplyService(IConfiguration configuration)
        {
            Init(configuration);
        }

        private void Init(IConfiguration config)
        {
            var autoReplyData = new Dictionary<Regex, string>();
            var statsData = new Dictionary<string, uint>();

            foreach (var item in config.GetSection("AutoReply").GetChildren())
            {
                var regexConfig = item.GetSection("IsInMessage");
                var rawRegex = regexConfig["Regex"];

                var regex = new Regex(rawRegex, (RegexOptions)Convert.ToInt32(regexConfig["OptionsFlags"]));

                autoReplyData.Add(regex, item["Reply"]);
            }

            Stats = statsData;
            AutoReplyData = autoReplyData;
        }

        public async Task TryReplyAsync(SocketUserMessage message)
        {
            var replyMessage = AutoReplyData.FirstOrDefault(o => o.Key.IsMatch(message.Content));

            if (!string.IsNullOrEmpty(replyMessage.Value))
            {
                var regex = replyMessage.Key.ToString();

                if (!Stats.ContainsKey(regex))
                    Stats.Add(regex, 1);
                else
                    Stats[regex]++;

                await message.Channel.SendMessageAsync(replyMessage.Value);
            }
        }

        public void ConfigChanged(IConfiguration newConfig)
        {
            Init(newConfig);
        }

        public Dictionary<string, uint> GetStatsData()
        {
            return Stats.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
        }
    }
}
