using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Services.Initiable;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
                using var repository = Provider.GetService<EmoteStatsRepository>();

                if (mentionedEmotes.Count == 0)
                {
                    TryIncrementUnicodeFromMessage(context.Message.Content, context.Guild, repository);
                    repository.SaveChanges();
                    return;
                }

                var serverEmotes = context.Guild.Emotes;
                foreach (var emote in mentionedEmotes)
                {
                    if (emote is Emoji emoji)
                    {
                        IncrementCounter(emoji.Name, true, context.Guild, repository);
                    }
                    else
                    {
                        var emoteId = emote.ToString();

                        if (serverEmotes.Any(o => o.ToString() == emoteId))
                            IncrementCounter(emoteId, false, context.Guild, repository);
                    }
                }

                repository.SaveChanges();
            }
        }

        public void IncrementFromReaction(SocketReaction reaction)
        {
            if (!(reaction.Channel is SocketGuildChannel channel)) return;
            if (!reaction.User.IsSpecified || !reaction.User.Value.IsUser()) return;

            var serverEmotes = channel.Guild.Emotes;

            lock (Locker)
            {
                using var repository = Provider.GetService<EmoteStatsRepository>();

                if (reaction.Emote is Emoji emoji)
                {
                    IncrementCounter(emoji.Name, true, channel.Guild, repository);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (serverEmotes.Any(o => o.ToString() == emoteId))
                        IncrementCounter(reaction.Emote.ToString(), false, channel.Guild, repository);
                }

                repository.SaveChanges();
            }
        }

        public void DecrementFromReaction(SocketReaction reaction)
        {
            if (!(reaction.Channel is SocketGuildChannel channel)) return;
            if (!reaction.User.IsSpecified || !reaction.User.Value.IsUser()) return;

            var serverEmotes = channel.Guild.Emotes;
            lock (Locker)
            {
                using var repository = Provider.GetService<EmoteStatsRepository>();

                if (reaction.Emote is Emoji emoji)
                {
                    DecrementCounter(reaction.Emote.Name, true, channel.Guild, repository);
                }
                else
                {
                    var emoteId = reaction.Emote.ToString();

                    if (serverEmotes.Any(o => o.ToString() == emoteId))
                        DecrementCounter(emoteId, false, channel.Guild, repository);
                }

                repository.SaveChanges();
            }
        }

        private void TryIncrementUnicodeFromMessage(string content, SocketGuild guild, EmoteStatsRepository repository)
        {
            var emojis = content
                .Split(' ')
                .Where(o => NeoSmart.Unicode.Emoji.IsEmoji(o))
                .Select(o => o.Trim());

            foreach (var emoji in emojis)
            {
                IncrementCounter(emoji, true, guild, repository);
            }
        }

        private void IncrementCounter(string emoteId, bool isUnicode, SocketGuild guild, EmoteStatsRepository repository)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            repository.AddOrIncrementEmoteNoCommit(guild, emoteId, isUnicode);
        }

        private void DecrementCounter(string emoteId, bool isUnicode, SocketGuild guild, EmoteStatsRepository repository)
        {
            if (isUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(emoteId);
                emoteId = Convert.ToBase64String(bytes);
            }

            repository.DecrementEmote(guild, emoteId);
        }

        public EmoteStat GetValue(SocketGuild guild, string emoteId)
        {
            using var repository = Provider.GetService<EmoteStatsRepository>();
            return repository.GetEmoteStat(guild, emoteId);
        }

        public List<EmoteStat> GetAllValues(bool descOrder, ulong guildID, bool excludeUnicode)
        {
            using var repository = Provider.GetService<EmoteStatsRepository>();
            var query = repository.GetEmoteStats(guildID, excludeUnicode);

            if (descOrder)
                return query.OrderByDescending(o => o.Count).ThenByDescending(o => o.LastOccuredAt).ToList();
            else
                return query.OrderBy(o => o.Count).ThenBy(o => o.LastOccuredAt).ToList();
        }

        public List<EmoteMergeListItem> GetMergeList(SocketGuild guild)
        {
            var emotes = GetAllValues(true, guild.Id, true);
            var data = guild.Emotes.Select(o => new EmoteMergeListItem() { Emote = o }).ToList();

            foreach (var emote in emotes)
            {
                var emoteData = Emote.Parse(emote.GetRealId());
                var serverEmote = data.Find(o => o.Emote.Id == emoteData.Id);

                if (serverEmote != null && emoteData.Name != serverEmote.Emote.Name)
                {
                    serverEmote.Emotes.Add(emoteData.ToString(), emote.Count);
                }
            }

            return data.FindAll(o => o.Emotes.Count > 0);
        }

        public void MergeEmotes(SocketGuild guild)
        {
            lock (Locker)
            {
                using var repository = Provider.GetService<EmoteStatsRepository>();

                foreach (var item in GetMergeList(guild))
                {
                    repository.MergeEmotes(guild, item);
                }
            }
        }

        public async Task<List<string>> CleanOldEmotesAsync(SocketGuild guild)
        {
            await guild.SyncGuildAsync().ConfigureAwait(false);

            lock (Locker)
            {
                using var repository = Provider.GetService<EmoteStatsRepository>();
                var removed = new List<string>();

                var emoteClearCandidates = repository.GetEmoteStats(guild.Id, true).ToList();

                foreach(var candidate in emoteClearCandidates)
                {
                    var parsedEmote = Emote.Parse(candidate.GetRealId());

                    if(!guild.Emotes.Any(o => o.Id == parsedEmote.Id))
                    {
                        removed.Add($"Smazán starý emote **{parsedEmote.Name}** ({parsedEmote.Id})");
                        repository.RemoveEmojiNoCommit(guild, candidate.GetRealId());
                    }
                }

                repository.SaveChanges();
                return removed;
            }
        }
    }
}
