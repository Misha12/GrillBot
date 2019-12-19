using Discord;
using Grillbot.Repository.Entity;
using Grillbot.Repository.Entity.UnverifyLog;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            List<ChannelOverride> overrides, string reason)
        {
            var entity = new TempUnverifyItem()
            {
                DeserializedRolesToReturn = roles,
                TimeFor = timeFor,
                UserID = userID.ToString(),
                GuildID = guildID.ToString(),
                DeserializedChannelOverrides = overrides,
                Reason = reason
            };

            await Context.TempUnverify.AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return entity;
        }

        public async Task<TempUnverifyItem> FindItemByIDAsync(int id)
        {
            return await GetAllItems().FirstOrDefaultAsync(o => o.ID == id).ConfigureAwait(false);
        }

        public async Task<TempUnverifyItem> FindUnverifyByUserID(ulong userId)
        {
            string user = userId.ToString();

            return await GetAllItems().FirstOrDefaultAsync(o => o.UserID == user).ConfigureAwait(false);
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
            var item = await FindItemByIDAsync(id).ConfigureAwait(false);

            if (item == null)
                return;

            Context.TempUnverify.Remove(item);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task UpdateTimeAsync(int id, long time)
        {
            var item = await FindItemByIDAsync(id).ConfigureAwait(false);

            if (item == null)
                return;

            item.StartAt = DateTime.Now;
            item.TimeFor = time;

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task LogOperationAsync(UnverifyLogOperation operation, IUser fromUser, IGuild guild, UnverifyLogDataBase data)
        {
            var entity = new UnverifyLog()
            {
                Data = JsonConvert.SerializeObject(data),
                DateTime = DateTime.Now,
                GuildID = guild.Id.ToString(),
                Operation = operation,
                FromUserID = fromUser.Id.ToString()
            };

            await Context.UnverifyLog.AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
