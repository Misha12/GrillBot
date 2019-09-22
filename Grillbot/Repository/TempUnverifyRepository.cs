using Discord.WebSocket;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Repository
{
    public class TempUnverifyRepository : RepositoryBase
    {
        public TempUnverifyRepository(Configuration config) : base(config)
        {
        }

        public IQueryable<TempUnverifyItem> GetAllItems()
        {
            return Context.TempUnverify.AsQueryable();
        }

        public async Task<TempUnverifyItem> AddItemAsync(List<string> roles, ulong userID, ulong guildID, long timeFor)
        {
            var entity = new TempUnverifyItem()
            {
                DeserializedRolesToReturn = roles,
                TimeFor = timeFor,
                UserID = userID.ToString(),
                GuildID = guildID.ToString()
            };

            await Context.TempUnverify.AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<TempUnverifyItem> FindItemByIDAsync(int id)
        {
            return await GetAllItems().FirstOrDefaultAsync(o => o.ID == id);
        }

        public async Task RemoveItemAsync(int id)
        {
            var item = await FindItemByIDAsync(id);

            if (item == null)
                return;

            Context.TempUnverify.Remove(item);
            await Context.SaveChangesAsync();
        }

        public async Task UpdateTimeAsync(int id, long time)
        {
            var item = await FindItemByIDAsync(id);

            if (item == null)
                return;

            item.StartAt = DateTime.Now;
            item.TimeFor = time;

            await Context.SaveChangesAsync();
        }
    }
}
