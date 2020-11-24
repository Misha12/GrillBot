using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Database.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Services.Initiable;
using Microsoft.Extensions.Logging;
using ReplyModel = Grillbot.Models.AutoReply.AutoReplyItem;
using Grillbot.Database;

namespace Grillbot.Modules.AutoReply
{
    public class AutoReplyService : IInitiable
    {
        private ILogger<AutoReplyService> Logger { get; }
        private BotState BotState { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        private AllowedMentions AllowedMentions { get; } = new AllowedMentions(AllowedMentionTypes.Users);

        public AutoReplyService(ILogger<AutoReplyService> logger, BotState botState, IGrillBotRepository grillBotRepository)
        {
            Logger = logger;
            BotState = botState;
            GrillBotRepository = grillBotRepository;
        }

        public void Init()
        {
            BotState.AutoReplyItems.Clear();
            BotState.AutoReplyItems.AddRange(GrillBotRepository.AutoReplyRepository.GetItems().ToList());
            Logger.LogInformation($"AutoReply module loaded (loaded {BotState.AutoReplyItems.Count} templates)");
        }

        public async Task TryReplyAsync(SocketGuild guild, SocketUserMessage message)
        {
            if (message.Channel is IPrivateChannel) return;

            bool replied = false;

            foreach (var item in BotState.AutoReplyItems)
            {
                if (item.CompareType == AutoReplyCompareTypes.Absolute)
                    replied = await TryReplyWithAbsolute(guild, message, item);
                else if (item.CompareType == AutoReplyCompareTypes.Contains)
                    replied = await TryReplyWithContains(guild, message, item);

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
            return BotState.AutoReplyItems
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
                        Channel = item.ChannelIDSnowflake == null ? "Kdekoliv" : channel
                    };
                }).ToList();
        }

        public async Task SetActiveStatusAsync(SocketGuild guild, int id, bool disabled)
        {
            var item = BotState.AutoReplyItems.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException("Hledaná odpověď nebyla nalezena.");

            if (item.IsDisabled == disabled)
                throw new ArgumentException("Tato automatická odpověd již má požadovaný stav.");

            var dbItem = await GrillBotRepository.AutoReplyRepository.FindItemByIdAsync(id);
            dbItem.IsDisabled = disabled;

            await GrillBotRepository.CommitAsync();
            item.IsDisabled = disabled;
        }

        public async Task AddReplyAsync(SocketGuild guild, string mustContains, string reply, string compareType, bool disabled, bool caseSensitive, string channel)
        {
            if (BotState.AutoReplyItems.Any(o => o.MustContains == mustContains))
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

            await GrillBotRepository.AddAsync(item);
            await GrillBotRepository.CommitAsync();
            BotState.AutoReplyItems.Add(item);
        }

        public async Task EditReplyAsync(SocketGuild guild, int id, string mustContains, string reply, string compareType, bool caseSensitive, string channel)
        {
            var item = BotState.AutoReplyItems.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            var dbItem = await GrillBotRepository.AutoReplyRepository.FindItemByIdAsync(id);

            dbItem.MustContains = mustContains;
            dbItem.ReplyMessage = reply;
            dbItem.CaseSensitive = caseSensitive;
            dbItem.SetCompareType(compareType);
            dbItem.ChannelIDSnowflake = channel == "*" ? (ulong?)null : Convert.ToUInt64(channel);

            await GrillBotRepository.CommitAsync();

            item.MustContains = dbItem.MustContains;
            item.ReplyMessage = dbItem.ReplyMessage;
            item.CaseSensitive = dbItem.CaseSensitive;
            item.CompareType = dbItem.CompareType;
            item.ChannelIDSnowflake = dbItem.ChannelIDSnowflake;
        }

        public async Task RemoveReplyAsync(SocketGuild guild, int id)
        {
            if (!BotState.AutoReplyItems.Any(o => o.GuildIDSnowflake == guild.Id && o.ID == id))
                throw new ArgumentException($"Automatická odpověď s ID **{id}** neexistuje.");

            var entity = await GrillBotRepository.AutoReplyRepository.FindItemByIdAsync(id);
            GrillBotRepository.Remove(entity);
            await GrillBotRepository.CommitAsync();
            BotState.AutoReplyItems.RemoveAll(o => o.ID == id);
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
