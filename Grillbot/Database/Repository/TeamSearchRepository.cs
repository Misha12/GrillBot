using System.Collections.Generic;
using System.Linq;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(GrillBotContext context) : base(context)
        {
        }

        public List<TeamSearch> GetSearches(int[] ids)
        {
            var query = Queryable.Where(Context.TeamSearch, o => ids.Contains(o.Id));
            return query.ToList();
        }

        public List<TeamSearch> GetAllSearches(string channelID)
        {
            var query = Context.TeamSearch.AsQueryable();

            if (!string.IsNullOrEmpty(channelID))
                query = query.Where(o => o.ChannelId == channelID);

            return query.ToList();
        }

        public void AddSearch(ulong guildID, ulong userID, ulong channelID, ulong messageID)
        {
            var entity = new TeamSearch()
            {
                ChannelIDSnowflake = channelID,
                GuildIDSnowflake = guildID,
                MessageIDSnowflake = messageID,
                UserIDSnowflake = userID
            };

            Context.TeamSearch.Add(entity);
            Context.SaveChanges();
        }

        public void RemoveSearch(int id)
        {
            var entity = FindSearchByID(id);
            RemoveSearch(entity);
        }

        public void RemoveSearch(TeamSearch entity)
        {
            if (entity == null) return;
            Context.TeamSearch.Remove(entity);
            Context.SaveChanges();
        }

        public TeamSearch FindSearchByID(int id)
        {
            return Context.TeamSearch.FirstOrDefault(o => o.Id == id);
        }
    }
}