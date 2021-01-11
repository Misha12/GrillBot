using DiscordEmoji = Discord.Emoji;
using TagType = Discord.TagType;
using Emote = Discord.Emote;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Entity.Users;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.EmoteStats;
using NeoSmart.Unicode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grillbot.Database.Enums.Includes;
using Microsoft.EntityFrameworkCore;
using Grillbot.Database;

namespace Grillbot.Services.Statistics
{
    public class EmoteStats
    {
        private IGrillBotRepository GrillBotRepository { get; }
        private SearchService SearchService { get; }

        public EmoteStats(IGrillBotRepository grillBotRepository, SearchService searchService)
        {
            GrillBotRepository = grillBotRepository;
            SearchService = searchService;
        }

        public async Task AnylyzeMessageAndIncrementValuesAsync(SocketCommandContext context)
        {
            if (context.Guild == null) return;

            var mentionedEmotes = context.Message.Tags.Where(o => o.Type == TagType.Emoji)
                .Select(o => o.Value)
                .DistinctBy(o => o.ToString());

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(context.Guild.Id, context.User.Id, UsersIncludes.Emotes);

            TryIncrementUnicodeFromMessage(context.Message.Content, userEntity);
            foreach (var emote in mentionedEmotes)
            {
                if (emote is DiscordEmoji emoji)
                {
                    IncrementCounter(emoji.Name, true, userEntity);
                }
                else
                {
                    var emoteId = emote.ToString();

                    if (context.Guild.Emotes.Any(o => o.ToString() == emoteId))
                        IncrementCounter(emoteId, false, userEntity);
                }
            }

            await GrillBotRepository.CommitAsync();
        }

        public async Task IncrementFromReactionAsync(SocketReaction reaction)
        {
            if (reaction.Channel is not SocketGuildChannel channel) return;

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(channel.Guild.Id, reaction.UserId, UsersIncludes.Emotes);

            if (reaction.Emote is DiscordEmoji emoji)
            {
                IncrementCounter(emoji.Name, true, userEntity);
            }
            else
            {
                var emoteId = reaction.Emote.ToString();

                if (channel.Guild.Emotes.Any(o => o.ToString() == emoteId))
                    IncrementCounter(reaction.Emote.ToString(), false, userEntity);
            }

            await GrillBotRepository.CommitAsync();
        }

        public async Task DecrementFromReactionAsync(SocketReaction reaction)
        {
            if (reaction.Channel is not SocketGuildChannel channel) return;
            if (!reaction.User.IsSpecified || !reaction.User.Value.IsUser()) return;

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(channel.Guild.Id, reaction.UserId, UsersIncludes.Emotes);

            if (reaction.Emote is DiscordEmoji emoji)
            {
                DecrementCounter(reaction.Emote.Name, true, userEntity);
            }
            else
            {
                var emoteId = reaction.Emote.ToString();

                if (channel.Guild.Emotes.Any(o => o.ToString() == emoteId))
                    DecrementCounter(emoteId, false, userEntity);
            }

            await GrillBotRepository.CommitAsync();
        }

        private void TryIncrementUnicodeFromMessage(string content, DiscordUser user)
        {
            var emojis = content.Codepoints()
                .Where(o => Emoji.IsKnownEmoji(o))
                .Distinct()
                .Select(o => o.AsString().Trim());

            foreach (var emoji in emojis)
            {
                IncrementCounter(emoji, true, user);
            }
        }

        private void IncrementCounter(string emoteId, bool isUnicode, DiscordUser user)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            var userEmote = user.UsedEmotes.FirstOrDefault(o => o.EmoteID == emoteId);

            if (userEmote == null)
            {
                userEmote = new EmoteStatItem()
                {
                    EmoteID = emoteId,
                    FirstOccuredAt = DateTime.Now,
                    IsUnicode = isUnicode,
                    LastOccuredAt = DateTime.Now,
                    UseCount = 1,
                };

                user.UsedEmotes.Add(userEmote);
            }
            else
            {
                userEmote.UseCount++;
                userEmote.LastOccuredAt = DateTime.Now;
            }
        }

        private void DecrementCounter(string emoteId, bool isUnicode, DiscordUser user)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            var userEmote = user.UsedEmotes.FirstOrDefault(o => o.EmoteID == emoteId);

            if (userEmote == null || userEmote.UseCount == 0)
                return;

            userEmote.UseCount--;
        }

        public GroupedEmoteItem GetValue(SocketGuild guild, string emoteId)
        {
            return GrillBotRepository.EmoteStatsRepository.GetStatsOfEmote(guild.Id, emoteId);
        }

        public List<GroupedEmoteItem> GetAllValues(bool descOrder, ulong guildID, bool excludeUnicode, int? limit = null)
        {
            return GrillBotRepository.EmoteStatsRepository.GetStatsOfEmotes(guildID, limit, excludeUnicode, descOrder).ToList();
        }

        public List<GroupedEmoteItem> GetAllUnicodeValues(bool descOrder, ulong guildID, int? limit = null)
        {
            return GrillBotRepository.EmoteStatsRepository.GetStatsOfEmotes(guildID, limit, false, descOrder, true).ToList();
        }

        public async Task<List<string>> CleanOldEmotesAsync(SocketGuild guild)
        {
            await guild.SyncGuildAsync();

            var emoteClearCandidates = GrillBotRepository.EmoteStatsRepository.GetEmotesForClear(guild.Id, 14);

            if (emoteClearCandidates.Count == 0)
                return new List<string>();

            var removed = new List<string>();
            foreach (var candidate in emoteClearCandidates)
            {
                if (candidate.IsUnicode)
                {
                    var formatedFirstOccured = candidate.FirstOccuredAt.ToLocaleDatetime();
                    var formatedLastOccured = candidate.LastOccuredAt.ToLocaleDatetime();

                    removed.Add($"> Smazán unicode emote **{candidate.RealID}**. Použití: 0, Poprvé použit: {formatedFirstOccured}, Naposledy použit: {formatedLastOccured}");
                    await GrillBotRepository.EmoteStatsRepository.RemoveEmojiNoCommitAsync(guild, candidate.EmoteID);
                    continue;
                }

                var parsedEmote = Emote.Parse(candidate.RealID);
                if (!guild.Emotes.Any(o => o.Id == parsedEmote.Id))
                {
                    removed.Add($"> Smazán starý emote **{parsedEmote.Name}** ({parsedEmote.Id}). Použito {candidate.UseCount.FormatWithSpaces()}x.");
                    await GrillBotRepository.EmoteStatsRepository.RemoveEmojiNoCommitAsync(guild, candidate.RealID);
                }
            }

            await GrillBotRepository.CommitAsync();
            return removed;
        }

        public async Task<List<EmoteStatItem>> GetEmoteStatsForUserAsync(SocketGuild guild, Discord.IUser user, bool desc)
        {
            var userId = await SearchService.GetUserIDFromDiscordUserAsync(guild, user);

            if (userId == null)
                return new List<EmoteStatItem>();

            var query = GrillBotRepository.EmoteStatsRepository.GetEmotesOfUser(userId.Value);

            if (desc)
                query = query.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccuredAt);
            else
                query = query.OrderBy(o => o.UseCount).ThenBy(o => o.LastOccuredAt);

            return await query.ToListAsync();
        }
    }
}
