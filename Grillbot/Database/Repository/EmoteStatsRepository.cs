using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Users;
using Grillbot.Models.EmoteStats;
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

        private IQueryable<EmoteStatItem> GetEmoteStatsBaseQuery(ulong guildID)
        {
            var userIDsFromGuild = GetUserIDsWithUsedEmotes(guildID);

            return Context.EmoteStatistics.AsQueryable()
                .Where(o => userIDsFromGuild.Contains(o.UserID));
        }

        public GroupedEmoteItem GetStatsOfEmote(ulong guildID, string emoteId)
        {
            return GetEmoteStatsBaseQuery(guildID)
                .Where(o => o.EmoteID == emoteId)
                .AsEnumerable()
                .GroupBy(o => o.EmoteID)
                .Select(o => new GroupedEmoteItem()
                {
                    EmoteID = o.Key,
                    FirstOccuredAt = o.Min(x => x.FirstOccuredAt),
                    IsUnicode = o.First().IsUnicode,
                    LastOccuredAt = o.Max(x => x.LastOccuredAt),
                    UseCount = o.Sum(x => x.UseCount),
                    UsersCount = o.Count()
                })
                .FirstOrDefault();
        }

        public IQueryable<GroupedEmoteItem> GetStatsOfEmotes(ulong guildID, int? limit, bool excludeUnicode, bool desc, bool onlyUnicode = false)
        {
            var query = GetEmoteStatsBaseQuery(guildID);

            if (onlyUnicode)
                query = query.Where(o => o.IsUnicode);
            else if (excludeUnicode)
                query = query.Where(o => !o.IsUnicode);

            var resultQuery = query.AsEnumerable()
                .GroupBy(o => o.EmoteID)
                .Select(o => new GroupedEmoteItem()
                {
                    EmoteID = o.Key,
                    FirstOccuredAt = o.Min(x => x.FirstOccuredAt),
                    IsUnicode = o.First().IsUnicode,
                    LastOccuredAt = o.Max(x => x.LastOccuredAt),
                    UseCount = o.Sum(x => x.UseCount),
                    UsersCount = o.Count()
                });

            if(desc)
            {
                resultQuery = resultQuery
                    .OrderByDescending(o => o.UseCount)
                    .ThenByDescending(o => o.UsersCount);
            }
            else
            {
                resultQuery = resultQuery
                    .OrderBy(o => o.UseCount)
                    .ThenBy(o => o.UsersCount);
            }

            if (limit != null)
                resultQuery = resultQuery.Take(limit.Value);

            return resultQuery.AsQueryable();
        }


        private List<long> GetUserIDsWithUsedEmotes(ulong guildID)
        {
            return Context.Users.AsQueryable()
                .Include(o => o.UsedEmotes)
                .Where(o => o.GuildID == guildID.ToString() && o.UsedEmotes.Any())
                .Select(o => o.ID)
                .ToList();
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

        public void RemoveEmojiNoCommit(SocketGuild guild, string emoteId)
        {
            var stat = GetEmoteStat(guild, emoteId);

            if (stat == null)
                return;

            Context.EmoteStats.Remove(stat);
        }
    }
}
