using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Grillbot.Repository.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Repository
{
    public class TeamSearchRepository : RepositoryBase
    {
        public TeamSearchRepository(IConfiguration config) : base(config)
        {
        }

        public async Task<IEnumerable<TeamSearch>> GetAllSearchesAsync()
        {
            return await Context.TeamSearch.ToArrayAsync();
        }
        
        public async Task AddSearch(IUser user, IChannel channel, ulong messageId)
        {
            await Context.TeamSearch.AddAsync(new TeamSearch
                            {UserId = user.Id.ToString(), MessageId = messageId.ToString(), ChannelId = channel.Id.ToString()});
            await Context.SaveChangesAsync();
        }
        
        public async Task RemoveSearch(int id)
        {
            var row = await Context.TeamSearch.FirstOrDefaultAsync(d => d.Id == id);
            if(row == null) return;
            Context.TeamSearch.Remove(row);
            await Context.SaveChangesAsync();
        }
    }
}