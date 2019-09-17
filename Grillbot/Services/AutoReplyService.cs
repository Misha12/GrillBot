using Discord.WebSocket;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public class AutoReplyService : IConfigChangeable
    {
        private List<AutoReplyItem> Data { get; set; }
        private BotLoggingService BotLogging { get; }
        private Configuration Config { get; set; }

        public AutoReplyService(IOptions<Configuration> configuration, BotLoggingService botLogging)
        {
            Data = new List<AutoReplyItem>();
            BotLogging = botLogging;
            Config = configuration.Value;

            Init();
        }

        private void Init()
        {
            Data.Clear();

            using(var repository = new AutoReplyRepository(Config))
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

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
            Init();
        }

        public Dictionary<string, int> GetStatsData()
        {
            return Data
                .OrderByDescending(o => o.CallsCount)
                .ToDictionary(o => o.MustContains, o => o.CallsCount);
        }

        public List<string> ListItems() => Data.Select(o => o.ToString()).ToList();

        public async Task SetActiveStatusAsync(int id, bool disabled)
        {
            var item = Data.FirstOrDefault(o => o.ID == id);

            if (item == null)
                throw new ArgumentException("Hledaná odpověď nebyla nalezena.");

            using(var repository = new AutoReplyRepository(Config))
            {
                await repository.SetActiveStatus(id, disabled);
            }

            if (item.IsDisabled == disabled)
                throw new ArgumentException("Tato automatická odpověd již má požadovaný stav.");

            item.IsDisabled = disabled;
        }

        public async Task AddReplyAsync(string mustContains, string reply, bool disabled = false)
        {
            if (Data.Any(o => o.MustContains == mustContains))
                throw new ArgumentException($"Automatická odpověď **{mustContains}** již existuje.");

            var item = new AutoReplyItem()
            {
                MustContains = mustContains,
                IsDisabled = disabled,
                ReplyMessage = reply
            };

            using(var repository = new AutoReplyRepository(Config))
            {
                await repository.AddItemAsync(item);
            }

            Data.Add(item);
        }

        public async Task EditReplyAsync(int id, string mustContains, string reply)
        {
            var item = Data.FirstOrDefault(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            using(var repository = new AutoReplyRepository(Config))
            {
                await repository.EditItemAsync(id, mustContains, reply);
            }

            item.MustContains = mustContains;
            item.ReplyMessage = reply;
        }

        public async Task RemoveReplyAsync(int id)
        {
            if (!Data.Any(o => o.ID == id))
                throw new ArgumentException($"Automatická odpověď s ID **{id}** neexistuje.");

            using(var repository = new AutoReplyRepository(Config))
            {
                await repository.RemoveItemAsync(id);
            }

            Data.RemoveAll(o => o.ID == id);
        }
    }
}
