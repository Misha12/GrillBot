using System;
using System.Linq;
using Discord.WebSocket;
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

        private IQueryable<EmoteStatItem> GetEmoteStatsBaseQuery(ulong guildID)
        {
            return Context.EmoteStatistics.AsQueryable()
                .Include(o => o.User)
                .Where(o => o.User.GuildID == guildID.ToString());
        }

        public GroupedEmoteItem GetStatsOfEmote(ulong guildID, string emoteId)
        {
            return FilterUserFromQuery(GetEmoteStatsBaseQuery(guildID).Where(o => o.EmoteID == emoteId))
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

        public IQueryable<EmoteStatItem> GetEmotesForClear(ulong guildID, int daysLimit)
        {
            var daysBack = DateTime.Now.AddDays(-daysLimit);

            var guildBaseQuery = FilterUserFromQuery(GetEmoteStatsBaseQuery(guildID));

            var nonUnicodeEmotes = guildBaseQuery.Where(o => !o.IsUnicode);

            var unicodeEmotes = guildBaseQuery
                .Where(o => o.IsUnicode)
                .GroupBy(o => o.EmoteID)
                .Where(o => o.Sum(x => x.UseCount) == 0 && o.Max(x => x.LastOccuredAt) <= daysBack)
                .Select(o => new EmoteStatItem()
                {
                    EmoteID = o.Key,
                    FirstOccuredAt = o.Min(x => x.FirstOccuredAt),
                    LastOccuredAt = o.Max(x => x.LastOccuredAt),
                    UseCount = o.Sum(x => x.UseCount),
                    UserID = o.Min(x => x.UserID),
                    IsUnicode = o.Min(x => x.IsUnicode ? 1 : 0) == 1
                });

            var data = nonUnicodeEmotes.ToList();
            data.AddRange(unicodeEmotes.ToList());

            return data.AsQueryable();
        }

        public IQueryable<GroupedEmoteItem> GetStatsOfEmotes(ulong guildID, int? limit, bool excludeUnicode, bool? desc, bool onlyUnicode = false)
        {
            var query = GetEmoteStatsBaseQuery(guildID);

            if (onlyUnicode)
                query = query.Where(o => o.IsUnicode);
            else if (excludeUnicode)
                query = query.Where(o => !o.IsUnicode);

            var resultQuery = FilterUserFromQuery(query).AsEnumerable()
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

            if (desc != null)
            {
                if (desc == true)
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
            }

            if (limit != null)
                resultQuery = resultQuery.Take(limit.Value);

            return resultQuery.AsQueryable();
        }

        public void RemoveEmojiNoCommit(SocketGuild guild, string emoteId)
        {
            var baseQuery = GetEmoteStatsBaseQuery(guild.Id).Where(o => o.EmoteID == emoteId);
            var emotes = FilterUserFromQuery(baseQuery).ToList();

            if (emotes.Count == 0)
                return;

            Context.EmoteStatistics.RemoveRange(emotes);
        }

        private IQueryable<EmoteStatItem> FilterUserFromQuery(IQueryable<EmoteStatItem> query)
        {
            return query.Select(o => new EmoteStatItem()
            {
                EmoteID = o.EmoteID,
                FirstOccuredAt = o.FirstOccuredAt,
                LastOccuredAt = o.LastOccuredAt,
                IsUnicode = o.IsUnicode,
                UseCount = o.UseCount,
                UserID = o.UserID
            });
        }

        public IQueryable<EmoteStatItem> GetEmotesOfUser(long userId)
        {
            return Context.EmoteStatistics.AsQueryable()
                .Where(o => o.UserID == userId && o.UseCount > 0);
        }
    }
}
