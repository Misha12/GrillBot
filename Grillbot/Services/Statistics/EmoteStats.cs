using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Entity.Users;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.EmoteStats;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Services.Statistics
{
    public class EmoteStats
    {
        private static object Locker { get; } = new object();
        private IServiceProvider Provider { get; }

        public EmoteStats(IServiceProvider provider)
        {
            Provider = provider;
        }

        public void AnylyzeMessageAndIncrementValues(SocketCommandContext context)
        {
            if (context.Guild == null) return;

            var mentionedEmotes = context.Message.Tags
                    .Where(o => o.Type == TagType.Emoji)
                    .Select(o => o.Value)
                    .DistinctBy(o => o.ToString())
                    .ToList();

            lock (Locker)
            {
                using var scope = Provider.CreateScope();
                using var userRepository = scope.ServiceProvider.GetService<UsersRepository>();

                var userEntity = userRepository.GetOrCreateUser(context.Guild.Id, context.User.Id, false, false, false, false, false, false, true);

                if (mentionedEmotes.Count == 0)
                {
                    TryIncrementUnicodeFromMessage(context.Message.Content, userEntity);
                    userRepository.SaveChanges();
                    return;
                }

                foreach (var emote in mentionedEmotes)
                {
                    if (emote is Emoji emoji)
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

                userRepository.SaveChanges();
            }
        }

        public void IncrementFromReaction(SocketReaction reaction)
        {
            if (!(reaction.Channel is SocketGuildChannel channel)) return;
            if (!reaction.User.IsSpecified || !reaction.User.Value.IsUser()) return;

            lock (Locker)
            {
                using var scope = Provider.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();

                var userEntity = repository.GetOrCreateUser(channel.Guild.Id, reaction.UserId, false, false, false, false, false, false, true);

                if (reaction.Emote is Emoji emoji)
                {
                    IncrementCounter(emoji.Name, true, userEntity);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (channel.Guild.Emotes.Any(o => o.ToString() == emoteId))
                        IncrementCounter(reaction.Emote.ToString(), false, userEntity);
                }

                repository.SaveChanges();
            }
        }

        public void DecrementFromReaction(SocketReaction reaction)
        {
            if (!(reaction.Channel is SocketGuildChannel channel)) return;
            if (!reaction.User.IsSpecified || !reaction.User.Value.IsUser()) return;

            lock (Locker)
            {
                using var scope = Provider.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();

                var userEntity = repository.GetOrCreateUser(channel.Guild.Id, reaction.UserId, false, false, false, false, false, false, true);

                if (reaction.Emote is Emoji emoji)
                {
                    DecrementCounter(reaction.Emote.Name, true, userEntity);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (channel.Guild.Emotes.Any(o => o.ToString() == emoteId))
                        DecrementCounter(emoteId, false, userEntity);
                }

                repository.SaveChanges();
            }
        }

        private void TryIncrementUnicodeFromMessage(string content, DiscordUser user)
        {
            var emojis = content
                .Split(' ')
                .Where(o => NeoSmart.Unicode.Emoji.IsEmoji(o))
                .Select(o => o.Trim());

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
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<EmoteStatsRepository>();

            return repository.GetStatsOfEmote(guild.Id, emoteId);
        }

        public List<GroupedEmoteItem> GetAllValues(bool descOrder, ulong guildID, bool excludeUnicode, int? limit = null)
        {
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<EmoteStatsRepository>();

            return repository.GetStatsOfEmotes(guildID, limit, excludeUnicode, descOrder).ToList();
        }

        public List<GroupedEmoteItem> GetAllUnicodeValues(bool descOrder, ulong guildID, int? limit = null)
        {
            using var scope = Provider.CreateScope();
            using var repository = scope.ServiceProvider.GetService<EmoteStatsRepository>();

            return repository.GetStatsOfEmotes(guildID, limit, false, descOrder, true).ToList();
        }

        public async Task<List<string>> CleanOldEmotesAsync(SocketGuild guild)
        {
            await guild.SyncGuildAsync();

            lock (Locker)
            {
                using var scope = Provider.CreateScope();
                using var repository = scope.ServiceProvider.GetService<EmoteStatsRepository>();
                var removed = new List<string>();

                var emoteClearCandidates = repository.GetStatsOfEmotes(guild.Id, null, false, true).ToList();

                if (emoteClearCandidates.Count == 0)
                    return new List<string>();

                foreach (var candidate in emoteClearCandidates)
                {
                    if (candidate.IsUnicode)
                    {
                        if (candidate.UseCount > 0)
                            continue;

                        var lastUsedDelta = DateTime.Now - candidate.LastOccuredAt;

                        if (lastUsedDelta.TotalDays >= 14.0)
                        {
                            var formatedFirstOccured = candidate.FirstOccuredAt.ToLocaleDatetime();
                            var formatedLastOccured = candidate.LastOccuredAt.ToLocaleDatetime();

                            removed.Add($"> Smazán unicode emote **{candidate.RealID}**. Použití: 0, Poprvé použit: {formatedFirstOccured}, Naposledy použit: {formatedLastOccured}");
                            repository.RemoveEmojiNoCommit(guild, candidate.EmoteID);
                        }

                        continue;
                    }

                    var parsedEmote = Emote.Parse(candidate.RealID);
                    if (!guild.Emotes.Any(o => o.Id == parsedEmote.Id))
                    {
                        removed.Add($"> Smazán starý emote **{parsedEmote.Name}** ({parsedEmote.Id})");
                        repository.RemoveEmojiNoCommit(guild, candidate.RealID);
                    }
                }

                repository.SaveChanges();
                return removed;
            }
        }

        public List<EmoteStatItem> GetEmoteStatsForUser(SocketGuild guild, IUser user, bool desc)
        {
            using var scope = Provider.CreateScope();
            using var usersRepository = scope.ServiceProvider.GetService<UsersRepository>();

            var userEntity = usersRepository.GetUser(guild.Id, user.Id, false, false, false, false, false, false, true);

            if (userEntity == null)
                return new List<EmoteStatItem>();

            if (desc)
                return userEntity.UsedEmotes.OrderByDescending(o => o.UseCount).ToList();
            else
                return userEntity.UsedEmotes.OrderBy(o => o.UseCount).ToList();
        }
    }
}
