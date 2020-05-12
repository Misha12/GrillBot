using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Models;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class EmoteStatsRepository : RepositoryBase
    {
        public EmoteStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public void AddOrIncrementEmoteNoCommit(SocketGuild guild, string emote, bool isUnicode)
        {
            var guildID = guild.Id.ToString();
            var stat = Context.EmoteStats.FirstOrDefault(o => o.GuildID == guildID && o.EmoteID == emote);

            if (stat == null)
            {
                stat = new EmoteStat()
                {
                    Count = 1,
                    EmoteID = emote,
                    GuildID = guildID,
                    IsUnicode = isUnicode,
                    LastOccuredAt = DateTime.Now
                };

                Context.EmoteStats.Add(stat);
            }
            else
            {
                stat.Count++;
                stat.LastOccuredAt = DateTime.Now;
            }
        }

        public void DecrementEmote(SocketGuild guild, string emote)
        {
            var guildID = guild.Id.ToString();
            var stat = Context.EmoteStats.FirstOrDefault(o => o.GuildID == guildID && o.EmoteID == emote);

            if (stat == null)
                return;

            stat.Count--;
        }

        public EmoteStat GetEmoteStat(SocketGuild guild, string emoteId)
        {
            var guildID = guild.Id.ToString();

            return Context.EmoteStats
                .FirstOrDefault(o => o.GuildID == guildID && o.EmoteID == emoteId);
        }

        public IQueryable<EmoteStat> GetEmoteStats(ulong guildID, bool excludeUnicode)
        {
            var query = Context.EmoteStats.AsQueryable()
                .Where(o => o.GuildID == guildID.ToString());

            if (excludeUnicode)
                query = query.Where(o => !o.IsUnicode);

            return query;
        }

        public void MergeEmotes(SocketGuild guild, EmoteMergeListItem item)
        {
            var guildID = guild.Id.ToString();
            var destination = Context.EmoteStats.FirstOrDefault(o => o.GuildID == guildID && o.EmoteID == item.MergeTo);

            bool isNew = false;
            if (destination == null)
            {
                destination = new EmoteStat()
                {
                    Count = 0,
                    EmoteID = item.MergeTo,
                    IsUnicode = false,
                    GuildID = guildID,
                    LastOccuredAt = DateTime.MinValue
                };

                isNew = true;
            }

            foreach (var source in item.Emotes)
            {
                destination.Count += source.Value;

                var oldEmote = Context.EmoteStats.FirstOrDefault(o => o.GuildID == guildID && o.EmoteID == source.Key);
                if (oldEmote == null) continue;

                Context.EmoteStats.Remove(oldEmote);
            }

            if (isNew)
                Context.EmoteStats.Add(destination);

            SaveChanges();
        }

        public void RemoveEmojiNoCommit(SocketGuild guild, string emoteId)
        {
            var stat = GetEmoteStat(guild, emoteId);

            if (stat == null)
                return;

            Context.EmoteStats.Remove(stat);
        }
    }
}
