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

        private AllowedMentions AllowedMentions { get; } = new AllowedMentions(AllowedMentionTypes.Users);

        public AutoReplyService(ILogger<AutoReplyService> logger, IServiceProvider provider)
        {
            Data = new List<AutoReplyItem>();
            Logger = logger;
            Provider = provider;
        }

        public void Init()
        {
            Data.Clear();

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<AutoReplyRepository>();
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

            if (item.CanReply(guild, message.Channel))
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(FormatMessage(item.ReplyMessage, message), allowedMentions: AllowedMentions);
                return true;
            }

            return false;
        }

        private async Task<bool> TryReplyWithAbsolute(SocketGuild guild, SocketUserMessage message, AutoReplyItem item)
        {
            if (!message.Content.Equals(item.MustContains, GetStringComparison(item)))
                return false;

            if (item.CanReply(guild, message.Channel))
            {
                item.CallsCount++;
                await message.Channel.SendMessageAsync(FormatMessage(item.ReplyMessage, message), allowedMentions: AllowedMentions);
                return true;
            }

            return false;
        }

        public List<ReplyModel> GetList(SocketGuild guild)
        {
            return Data
                .Where(o => o.GuildIDSnowflake == guild.Id)
                .OrderByDescending(o => o.CallsCount)
                .Select(item =>
                {
                    var channel = item.ChannelIDSnowflake == null ? null : guild.GetChannel(item.ChannelIDSnowflake.Value)?.Name ?? $"Neznámý ({item.ChannelIDSnowflake})";

                    return new ReplyModel()
                    {
                        CallsCount = item.CallsCount,
                        CaseSensitive = item.CaseSensitive,
                        CompareType = item.CompareType,
                        ID = item.ID,
                        IsActive = !item.IsDisabled,
                        MustContains = item.MustContains,
                        Reply = item.ReplyMessage,
                        Channel = item.ChannelIDSnowflake == null ? "Všude" : channel
                    };
                }).ToList();
        }

        public async Task SetActiveStatusAsync(SocketGuild guild, int id, bool disabled)
        {
            var item = Data.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException("Hledaná odpověď nebyla nalezena.");

            if (item.IsDisabled == disabled)
                throw new ArgumentException("Tato automatická odpověd již má požadovaný stav.");

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<AutoReplyRepository>();

            await repository.SetActiveStatusAsync(id, disabled).ConfigureAwait(false);
            item.IsDisabled = disabled;
        }

        public async Task AddReplyAsync(SocketGuild guild, string mustContains, string reply, string compareType, bool disabled, bool caseSensitive, string channel)
        {
            if (Data.Any(o => o.MustContains == mustContains))
                throw new ArgumentException($"Automatická odpověď **{mustContains}** již existuje.");

            var item = new AutoReplyItem()
            {
                MustContains = mustContains,
                IsDisabled = disabled,
                ReplyMessage = reply,
                CaseSensitive = caseSensitive,
                GuildIDSnowflake = guild.Id,
                ChannelIDSnowflake = channel == "*" ? (ulong?)null : Convert.ToUInt64(channel)
            };

            item.SetCompareType(compareType);

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<AutoReplyRepository>();
            await repository.AddItemAsync(item).ConfigureAwait(false);

            Data.Add(item);
        }

        public async Task EditReplyAsync(SocketGuild guild, int id, string mustContains, string reply, string compareType, bool caseSensitive, string channel)
        {
            var item = Data.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<AutoReplyRepository>();
            await repository.EditItemAsync(id, mustContains, reply, compareType, caseSensitive).ConfigureAwait(false);

            item.MustContains = mustContains;
            item.ReplyMessage = reply;
            item.CaseSensitive = caseSensitive;
            item.SetCompareType(compareType);
            item.ChannelIDSnowflake = channel == "*" ? (ulong?)null : Convert.ToUInt64(channel);
        }

        public async Task RemoveReplyAsync(SocketGuild guild, int id)
        {
            if (!Data.Any(o => o.GuildIDSnowflake == guild.Id && o.ID == id))
                throw new ArgumentException($"Automatická odpověď s ID **{id}** neexistuje.");

            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<AutoReplyRepository>();
            await repository.RemoveItemAsync(id).ConfigureAwait(false);
            Data.RemoveAll(o => o.ID == id);
        }

        private StringComparison GetStringComparison(AutoReplyItem item)
        {
            return !item.CaseSensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
        }

        private string FormatMessage(string message, IMessage originalMessage)
        {
            return message
                .PreventMassTags()
                .Replace("{author}", originalMessage.Author.Mention)
                .Trim();
        }

        public async Task InitAsync() { }
    }
}
