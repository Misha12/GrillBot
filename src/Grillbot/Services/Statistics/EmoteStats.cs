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
using Grillbot.Enums;

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

        public Task<GroupedEmoteItem> GetValueAsync(SocketGuild guild, string emoteId)
        {
            return GrillBotRepository.EmoteStatsRepository.GetStatsOfEmoteAsync(guild.Id, emoteId);
        }

        public List<GroupedEmoteItem> GetAllValues(SortType sortType, ulong guildID, bool excludeUnicode, EmoteInfoOrderType orderType, int? limit = null)
        {
            return GrillBotRepository.EmoteStatsRepository.GetStatsOfEmotes(guildID, limit, excludeUnicode, sortType, orderType).ToList();
        }

        public List<GroupedEmoteItem> GetAllUnicodeValues(SortType sortType, ulong guildID, int? limit = null)
        {
            return GrillBotRepository.EmoteStatsRepository.GetStatsOfEmotes(guildID, limit, false, sortType, EmoteInfoOrderType.Count, true).ToList();
        }

        public async Task<List<string>> CleanOldEmotesAsync(SocketGuild guild)
        {
            await guild.SyncGuildAsync();

            var emoteClearCandidates = await GrillBotRepository.EmoteStatsRepository.GetEmotesForClearAsync(guild.Id, 14);

            if (emoteClearCandidates.Count == 0)
                return new List<string>();

            var removed = new List<EmoteStatItem>();
            foreach (var candidate in emoteClearCandidates)
            {
                if (candidate.IsUnicode)
                {
                    removed.Add(candidate);
                    await GrillBotRepository.EmoteStatsRepository.RemoveEmojiNoCommitAsync(guild, candidate.EmoteID);
                    continue;
                }

                var parsedEmote = Emote.Parse(candidate.RealID);
                if (!guild.Emotes.Any(o => o.Id == parsedEmote.Id))
                {
                    removed.Add(candidate);
                    await GrillBotRepository.EmoteStatsRepository.RemoveEmojiNoCommitAsync(guild, candidate.RealID);
                }
            }

            await GrillBotRepository.CommitAsync();

            var typeGroup = removed
                .GroupBy(o => o.IsUnicode)
                .ToDictionary(o => o.Key, o => o.GroupBy(x => x.RealID));

            var messages = (typeGroup.TryGetValue(false, out var nonUnicode) ? nonUnicode : new List<IGrouping<string, EmoteStatItem>>()) // Non-unicode
                .Select(o =>
                {
                    var emote = Emote.Parse(o.Key);
                    return $"> Smazán starý emote **{emote.Name}** ({emote.Id}). Použito: {o.Sum(x => x.UseCount).FormatWithSpaces()}x.";
                })
                .ToList();

            messages.AddRange((typeGroup.TryGetValue(true, out var unicode) ? unicode : new List<IGrouping<string, EmoteStatItem>>()) // Unicode
                .Select(o => $"> Smazán unicode emote **{o.Key}**. Použití: 0. Poprvé použit: {o.Min(x => x.FirstOccuredAt).ToLocaleDatetime()}. Naposledy použit: {o.Max(x => x.LastOccuredAt).ToLocaleDatetime()}"));

            return messages;
        }

        public async Task<List<EmoteStatItem>> GetEmoteStatsForUserAsync(SocketGuild guild, Discord.IUser user, SortType sortType)
        {
            var userId = await SearchService.GetUserIDFromDiscordUserAsync(guild, user);

            if (userId == null)
                return new List<EmoteStatItem>();

            var query = GrillBotRepository.EmoteStatsRepository.GetEmotesOfUser(userId.Value);

            if (sortType == SortType.Desc)
                query = query.OrderByDescending(o => o.UseCount).ThenByDescending(o => o.LastOccuredAt);
            else if(sortType == SortType.Asc)
                query = query.OrderBy(o => o.UseCount).ThenBy(o => o.LastOccuredAt);

            return await query.ToListAsync();
        }
    }
}
