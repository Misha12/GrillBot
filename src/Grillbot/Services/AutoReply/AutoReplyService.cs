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
using Grillbot.Models.AutoReply;
using Grillbot.Enums;

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
            BotState.AutoReplyItems = GrillBotRepository.AutoReplyRepository.GetItems().ToList();
            Logger.LogInformation($"AutoReply module loaded (loaded {BotState.AutoReplyItems.Count} templates)");
        }

        public async Task TryReplyAsync(SocketGuild guild, SocketUserMessage message)
        {
            bool replied = false;

            foreach (var item in BotState.AutoReplyItems.Where(o => o.CanReply(guild, message.Channel)))
            {
                if (item.CompareType == AutoReplyCompareTypes.Absolute)
                    replied = await TryReplyWithAbsolute(message, item);
                else if (item.CompareType == AutoReplyCompareTypes.Contains)
                    replied = await TryReplyWithContains(message, item);

                if (replied)
                    break;
            }
        }

        private async Task<bool> TryReplyWithContains(SocketUserMessage message, Database.Entity.AutoReplyItem item)
        {
            if (!message.Content.Contains(item.MustContains, item.StringComparison))
                return false;

            item.CallsCount++;
            await message.Channel.SendMessageAsync(FormatMessage(item, message), allowedMentions: AllowedMentions);
            return true;
        }

        private async Task<bool> TryReplyWithAbsolute(SocketUserMessage message, Database.Entity.AutoReplyItem item)
        {
            if (!message.Content.Equals(item.MustContains, item.StringComparison))
                return false;

            item.CallsCount++;
            await message.Channel.SendMessageAsync(FormatMessage(item, message), allowedMentions: AllowedMentions);
            return true;
        }

        public List<ReplyModel> GetList(SocketGuild guild)
        {
            return BotState.AutoReplyItems
                .Where(o => o.GuildIDSnowflake == guild.Id)
                .OrderBy(o => o.ID)
                .Select(item =>
                {
                    var channel = item.ChannelIDSnowflake == null ? null : guild.GetChannel(item.ChannelIDSnowflake.Value)?.Name ?? $"Neznámý ({item.ChannelIDSnowflake})";

                    return new ReplyModel()
                    {
                        CallsCount = item.CallsCount,
                        CompareType = item.CompareType,
                        ID = item.ID,
                        MustContains = item.MustContains,
                        Reply = item.ReplyMessage,
                        Channel = item.ChannelIDSnowflake == null ? "Kdekoliv" : channel,
                        Flags = item.Flags
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

            if (disabled)
                dbItem.Flags |= (int)AutoReplyParams.Disabled;
            else
                dbItem.Flags &= ~(int)AutoReplyParams.Disabled;

            await GrillBotRepository.CommitAsync();
            item.Flags = dbItem.Flags;
        }

        public async Task AddReplyAsync(SocketGuild guild, AutoreplyData data)
        {
            if (BotState.AutoReplyItems.Any(o => o.MustContains == data.MustContains))
                throw new ArgumentException("Automatická odpověď s tímto řetězcem již existuje.");

            var item = data.ToEntity(guild);
            await GrillBotRepository.AddAsync(item);
            await GrillBotRepository.CommitAsync();
            BotState.AutoReplyItems.Add(item);
        }

        public async Task EditReplyAsync(SocketGuild guild, int id, AutoreplyData data)
        {
            var item = BotState.AutoReplyItems.Find(o => o.GuildIDSnowflake == guild.Id && o.ID == id);

            if (item == null)
                throw new ArgumentException($"Automatická odpověď s ID **{id}** nebyla nalezena.");

            var dbItem = await GrillBotRepository.AutoReplyRepository.FindItemByIdAsync(id);

            data.UpdateEntity(dbItem);
            await GrillBotRepository.CommitAsync();

            data.UpdateEntity(item);
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

        private string FormatMessage(Database.Entity.AutoReplyItem item, IMessage originalMessage)
        {
            var msg = item.ReplyMessage
                .PreventMassTags()
                .Replace("{author}", originalMessage.Author.Mention)
                .Trim();

            if ((item.Flags & (int)AutoReplyParams.AsCodeBlock) != 0)
                msg = $"```{msg}```";

            return msg;
        }

        public async Task InitAsync() { }
    }
}
