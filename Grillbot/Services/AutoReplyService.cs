using Discord;
using Discord.WebSocket;
using Grillbot.Helpers;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services;
using Grillbot.Services.Config;
using Grillbot.Services.Config.Models;
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
            bool replied = false;

            foreach(var item in Data)
            {
                if (item.CompareType == AutoReplyCompareTypes.Absolute)
                    replied = await TryReplyWithAbsolute(message, item);
                else if (item.CompareType == AutoReplyCompareTypes.Contains)
                    replied = await TryReplyWithContains(message, item);

                if (replied)
                    break;
            }
        }

        private async Task<bool> TryReplyWithContains(SocketUserMessage message, AutoReplyItem item)
        {
            if (!message.Content.Contains(item.MustContains))
                return false;

            if(item.CanReply())
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(item.ReplyMessage);
                return true;
            }

            return false;
        }

        private async Task<bool> TryReplyWithAbsolute(SocketUserMessage message, AutoReplyItem item)
        {
            if (message.Content != item.MustContains)
                return false;

            if(item.CanReply())
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(item.ReplyMessage);
                return true;
            }

            return false;
        }

        public void ConfigChanged(Configuration newConfig)
        {
            Config = newConfig;
            Init();
        }

        public Embed GetList(SocketUserMessage requestMessage)
        {
            if (Data.Count == 0)
                return null;

            var embedBuilder = new EmbedBuilder();

            embedBuilder
                .WithColor(Color.Blue)
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro: {requestMessage.Author.Username}#{requestMessage.Author.Discriminator}",
                    requestMessage.Author.GetAvatarUrl() ?? requestMessage.Author.GetDefaultAvatarUrl())
                .WithTitle("Automatické odpovědi");

            foreach(var item in Data)
            {
                embedBuilder.AddField(field =>
                {
                    var statusMessage = item.IsDisabled ? "Neaktivní" : "Aktivní";

                    field
                        .WithName($"**{item.ID}** - {item.MustContains}")
                        .WithValue($"Odpověď: {item.ReplyMessage}\nStatus: {statusMessage}\nMetoda: {item.CompareType}" +
                            $"\nPočet použití: {FormatHelper.FormatWithSpaces(item.CallsCount)}");
                });
            }

            return embedBuilder.Build();
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

        public async Task AddReplyAsync(string mustContains, string reply, string compareType, bool disabled = false)
        {
            if (Data.Any(o => o.MustContains == mustContains))
                throw new ArgumentException($"Automatická odpověď **{mustContains}** již existuje.");

            var item = new AutoReplyItem()
            {
                MustContains = mustContains,
                IsDisabled = disabled,
                ReplyMessage = reply
            };

            item.SetCompareType(compareType);

            using(var repository = new AutoReplyRepository(Config))
            {
                await repository.AddItemAsync(item);
            }

            Data.Add(item);
        }

        public async Task EditReplyAsync(int id, string mustContains, string reply, string compareType)
        {
            var item = Data.FirstOrDefault(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            using(var repository = new AutoReplyRepository(Config))
            {
                await repository.EditItemAsync(id, mustContains, reply, compareType);
            }

            item.MustContains = mustContains;
            item.ReplyMessage = reply;
            item.SetCompareType(compareType);
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
