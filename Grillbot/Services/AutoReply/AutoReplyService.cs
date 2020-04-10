using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Helpers;
using Grillbot.Models.Embed;
using Grillbot.Database.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Initiable;
using Grillbot.Database.Repository;
using Microsoft.Extensions.Logging;

namespace Grillbot.Modules.AutoReply
{
    public class AutoReplyService : IInitiable
    {
        private List<AutoReplyItem> Data { get; }
        private ILogger<AutoReplyService> Logger { get; }
        private AutoReplyRepository Repository { get; }

        public AutoReplyService(ILogger<AutoReplyService> logger, AutoReplyRepository repository)
        {
            Data = new List<AutoReplyItem>();
            Repository = repository;
            Logger = logger;
        }

        public void Init()
        {
            Data.Clear();
            Data.AddRange(Repository.GetAllItems());

            Logger.LogInformation($"AutoReply module loaded (loaded {Data.Count} templates)");
        }

        public async Task TryReplyAsync(SocketUserMessage message)
        {
            if (message.Channel is IPrivateChannel) return;

            bool replied = false;

            foreach (var item in Data)
            {
                if (item.CompareType == AutoReplyCompareTypes.Absolute)
                    replied = await TryReplyWithAbsolute(message, item).ConfigureAwait(false);
                else if (item.CompareType == AutoReplyCompareTypes.Contains)
                    replied = await TryReplyWithContains(message, item).ConfigureAwait(false);

                if (replied)
                    break;
            }
        }

        private async Task<bool> TryReplyWithContains(SocketUserMessage message, AutoReplyItem item)
        {
            if (!message.Content.Contains(item.MustContains, GetStringComparison(item)))
                return false;

            if (item.CanReply())
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(item.ReplyMessage.PreventMassTags()).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async Task<bool> TryReplyWithAbsolute(SocketUserMessage message, AutoReplyItem item)
        {
            if (!message.Content.Equals(item.MustContains, GetStringComparison(item)))
                return false;

            if (item.CanReply())
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(item.ReplyMessage.PreventMassTags()).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public Embed GetList(SocketUserMessage requestMessage)
        {
            if (Data.Count == 0)
                return null;

            var embed = new BotEmbed(requestMessage.Author)
                .WithTitle("Automatické odpovědi");

            foreach (var item in Data)
            {
                embed.AddField(field =>
                {
                    var statusMessage = item.IsDisabled ? "Neaktivní" : "Aktivní";

                    field
                        .WithName($"**{item.ID}** - {item.MustContains}")
                        .WithValue(string.Join("\n", new[]
                        {
                            $"Odpověď: {item.ReplyMessage}",
                            $"Status: {statusMessage}",
                            $"Metoda: {item.CompareType}",
                            $"Počet použití: {FormatHelper.FormatWithSpaces(item.CallsCount)}",
                            $"Case sensitive: {(item.CaseSensitive ? "Ano" : "Ne")}"
                        }));
                });
            }

            return embed.Build();
        }

        public async Task SetActiveStatusAsync(int id, bool disabled)
        {
            var item = Data.Find(o => o.ID == id);

            if (item == null)
                throw new ArgumentException("Hledaná odpověď nebyla nalezena.");

            await Repository.SetActiveStatusAsync(id, disabled).ConfigureAwait(false);
            if (item.IsDisabled == disabled)
                throw new ArgumentException("Tato automatická odpověd již má požadovaný stav.");

            item.IsDisabled = disabled;
        }

        public async Task AddReplyAsync(string mustContains, string reply, string compareType, bool disabled = false, bool caseSensitive = false)
        {
            if (Data.Any(o => o.MustContains == mustContains))
                throw new ArgumentException($"Automatická odpověď **{mustContains}** již existuje.");

            var item = new AutoReplyItem()
            {
                MustContains = mustContains,
                IsDisabled = disabled,
                ReplyMessage = reply,
                CaseSensitive = caseSensitive
            };

            item.SetCompareType(compareType);
            await Repository.AddItemAsync(item).ConfigureAwait(false);
            Data.Add(item);
        }

        public async Task EditReplyAsync(int id, string mustContains, string reply, string compareType, bool caseSensitive)
        {
            var item = Data.Find(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            await Repository.EditItemAsync(id, mustContains, reply, compareType, caseSensitive).ConfigureAwait(false);

            item.MustContains = mustContains;
            item.ReplyMessage = reply;
            item.CaseSensitive = caseSensitive;
            item.SetCompareType(compareType);
        }

        public async Task RemoveReplyAsync(int id)
        {
            if (!Data.Any(o => o.ID == id))
                throw new ArgumentException($"Automatická odpověď s ID **{id}** neexistuje.");

            await Repository.RemoveItemAsync(id).ConfigureAwait(false);
            Data.RemoveAll(o => o.ID == id);
        }

        private StringComparison GetStringComparison(AutoReplyItem item)
        {
            return !item.CaseSensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        }

        public List<AutoReplyItem> GetItems()
        {
            return Data
                .OrderByDescending(o => o.CallsCount)
                .ThenBy(o => o.ID)
                .ToList();
        }

        public async Task InitAsync() { }
    }
}
