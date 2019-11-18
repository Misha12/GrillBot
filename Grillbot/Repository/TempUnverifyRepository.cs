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

        public async Task<TempUnverifyItem> AddItemAsync(List<string> roles, ulong userID, ulong guildID, long timeFor, 
            List<ChannelOverride> overrides)
        {
            var entity = new TempUnverifyItem()
            {
                DeserializedRolesToReturn = roles,
                TimeFor = timeFor,
                UserID = userID.ToString(),
                GuildID = guildID.ToString(),
                DeserializedChannelOverrides = overrides
            };

            await Context.TempUnverify.AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<TempUnverifyItem> FindItemByIDAsync(int id)
        {
            return await GetAllItems().FirstOrDefaultAsync(o => o.ID == id);
        }

        public async Task<TempUnverifyItem> FindUnverifyByUserID(ulong userId)
        {
            string user = userId.ToString();

            return await GetAllItems().FirstOrDefaultAsync(o => o.UserID == user);
        }

        public void RemoveItem(int id)
        {
            var item = Context.TempUnverify.FirstOrDefault(o => o.ID == id);

            if (item == null)
                return;

            Context.TempUnverify.Remove(item);
            Context.SaveChanges();
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
