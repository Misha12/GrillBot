using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Database.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Initiable;
using Grillbot.Database.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ReplyModel = Grillbot.Models.AutoReply.AutoReplyItem;

namespace Grillbot.Modules.AutoReply
{
    public class AutoReplyService : IInitiable
    {
        private List<AutoReplyItem> Data { get; }
        private ILogger<AutoReplyService> Logger { get; }
        private IServiceProvider Provider { get; }

        public AutoReplyService(ILogger<AutoReplyService> logger, IServiceProvider provider)
        {
            Data = new List<AutoReplyItem>();
            Logger = logger;
            Provider = provider;
        }

        private AutoReplyRepository GetRepository()
        {
            return Provider.GetService<AutoReplyRepository>();
        }

        public void Init()
        {
            Data.Clear();

            using var repository = GetRepository();
            Data.AddRange(repository.GetAllItems());

            Logger.LogInformation($"AutoReply module loaded (loaded {Data.Count} templates)");
        }

        public async Task TryReplyAsync(SocketGuild guild, SocketUserMessage message)
        {
            if (message.Channel is IPrivateChannel) return;

            bool replied = false;

            foreach (var item in Data)
            {
                if (item.CompareType == AutoReplyCompareTypes.Absolute)
                    replied = await TryReplyWithAbsolute(guild, message, item).ConfigureAwait(false);
                else if (item.CompareType == AutoReplyCompareTypes.Contains)
                    replied = await TryReplyWithContains(guild, message, item).ConfigureAwait(false);

                if (replied)
                    break;
            }
        }

        private async Task<bool> TryReplyWithContains(SocketGuild guild, SocketUserMessage message, AutoReplyItem item)
        {
            if (!message.Content.Contains(item.MustContains, GetStringComparison(item)))
                return false;

            if (item.CanReply(guild))
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(item.ReplyMessage.PreventMassTags()).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        private async Task<bool> TryReplyWithAbsolute(SocketGuild guild, SocketUserMessage message, AutoReplyItem item)
        {
            if (!message.Content.Equals(item.MustContains, GetStringComparison(item)))
                return false;

            if (item.CanReply(guild))
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(item.ReplyMessage.PreventMassTags()).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        public List<ReplyModel> GetList(SocketGuild guild)
        {
            return Data
                .Where(o => o.GuildIDSnowflake == guild.Id)
                .OrderByDescending(o => o.CallsCount)
                .Select(item => new ReplyModel()
                {
                    CallsCount = item.CallsCount,
                    CaseSensitive = item.CaseSensitive,
                    CompareType = item.CompareType,
                    ID = item.ID,
                    IsActive = !item.IsDisabled,
                    MustContains = item.MustContains,
                    Reply = item.ReplyMessage
                }).ToList();
        }

        public async Task SetActiveStatusAsync(SocketGuild guild, int id, bool disabled)
        {
            var item = Data.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException("Hledaná odpověď nebyla nalezena.");

            using var repository = GetRepository();
            await repository.SetActiveStatusAsync(id, disabled).ConfigureAwait(false);
            if (item.IsDisabled == disabled)
                throw new ArgumentException("Tato automatická odpověd již má požadovaný stav.");

            item.IsDisabled = disabled;
        }

        public async Task AddReplyAsync(SocketGuild guild, string mustContains, string reply, string compareType, bool disabled = false, bool caseSensitive = false)
        {
            if (Data.Any(o => o.MustContains == mustContains))
                throw new ArgumentException($"Automatická odpověď **{mustContains}** již existuje.");

            var item = new AutoReplyItem()
            {
                MustContains = mustContains,
                IsDisabled = disabled,
                ReplyMessage = reply,
                CaseSensitive = caseSensitive,
                GuildIDSnowflake = guild.Id
            };

            item.SetCompareType(compareType);

            using var repository = GetRepository();
            await repository.AddItemAsync(item).ConfigureAwait(false);

            Data.Add(item);
        }

        public async Task EditReplyAsync(SocketGuild guild, int id, string mustContains, string reply, string compareType, bool caseSensitive)
        {
            var item = Data.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            using var repository = GetRepository();
            await repository.EditItemAsync(id, mustContains, reply, compareType, caseSensitive).ConfigureAwait(false);

            item.MustContains = mustContains;
            item.ReplyMessage = reply;
            item.CaseSensitive = caseSensitive;
            item.SetCompareType(compareType);
        }

        public async Task RemoveReplyAsync(SocketGuild guild, int id)
        {
            if (!Data.Any(o => o.GuildIDSnowflake == guild.Id && o.ID == id))
                throw new ArgumentException($"Automatická odpověď s ID **{id}** neexistuje.");

            using var repository = GetRepository();
            await repository.RemoveItemAsync(id).ConfigureAwait(false);
            Data.RemoveAll(o => o.ID == id);
        }

        private StringComparison GetStringComparison(AutoReplyItem item)
        {
            return !item.CaseSensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        }

        public async Task InitAsync() { }
    }
}
