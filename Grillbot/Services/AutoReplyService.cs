using Discord.WebSocket;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services;
using Grillbot.Services.Config;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public class AutoReplyService : IConfigChangeable
    {
        private List<AutoReplyItem> Data { get; set; }
        private BotLoggingService BotLogging { get; }

        public AutoReplyService(IConfiguration configuration, BotLoggingService botLogging)
        {
            Data = new List<AutoReplyItem>();
            BotLogging = botLogging;

            Init(configuration);
        }

        private void Init(IConfiguration config)
        {
            using(var repository = new AutoReplyRepository(config))
            {
                var autoReplyData = repository.GetAllItems();
                Data.AddRange(autoReplyData);

                BotLogging.WriteToLog($"AutoReply module loaded (loaded {Data.Count} templates)");
            }
        }

        public async Task TryReplyAsync(SocketUserMessage message)
        {
            var replyMessage = Data.FirstOrDefault(o => message.Content.Contains(o.MustContains));

            if (replyMessage?.CanReply() == true)
            {
                replyMessage.CallsCount++;
                await message.Channel.SendMessageAsync(replyMessage.ReplyMessage);
            }
        }

        public void ConfigChanged(IConfiguration newConfig) => Init(newConfig);

        public Dictionary<string, int> GetStatsData()
        {
            return Data.OrderByDescending(o => o.CallsCount).ToDictionary(o => o.MustContains, o => o.CallsCount);
        }
    }
}
