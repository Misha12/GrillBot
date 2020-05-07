using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.UnverifyLog;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class TempUnverifyRepository : RepositoryBase
    {
        public TempUnverifyRepository(GrillBotContext context) : base(context)
        {
        }

        public IQueryable<TempUnverifyItem> GetAllItems(SocketGuild guild)
        {
            var query = Context.TempUnverify.AsQueryable();

            if (guild != null)
            {
                var guildID = guild.Id.ToString();
                query = query.Where(o => o.GuildID == guildID);
            }

            return query.AsQueryable();
        }

        public async Task<TempUnverifyItem> AddItemAsync(List<ulong> roles, ulong userID, ulong guildID, int timeFor,
            List<ChannelOverride> overrides, string reason)
        {
            var entity = new TempUnverifyItem()
            {
                DeserializedRolesToReturn = roles,
                TimeFor = timeFor,
                UserIDSnowflake = userID,
                GuildIDSnowflake = guildID,
                DeserializedChannelOverrides = overrides,
                Reason = reason
            };

            await Context.TempUnverify.AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return entity;
        }

        public async Task<TempUnverifyItem> FindItemByIDAsync(int id)
        {
            return await GetAllItems(null).FirstOrDefaultAsync(o => o.ID == id).ConfigureAwait(false);
        }

        public async Task<TempUnverifyItem> FindUnverifyByUserID(ulong userId)
        {
            string user = userId.ToString();

            return await GetAllItems(null).FirstOrDefaultAsync(o => o.UserID == user).ConfigureAwait(false);
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

        public async Task UpdateTimeAsync(int id, int time)
        {
            var item = await FindItemByIDAsync(id).ConfigureAwait(false);

            if (item == null)
                return;

            item.StartAt = DateTime.Now;
            item.TimeFor = time;

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public void LogOperation(UnverifyLogOperation operation, IUser fromUser, IGuild guild, IUser toUser, object data)
        {
            var entity = new UnverifyLog()
            {
                Data = JsonConvert.SerializeObject(data),
                DateTime = DateTime.Now,
                GuildIDSnowflake = guild.Id,
                Operation = operation,
                FromUserIDSnowflake = fromUser.Id,
                DestUserIDSnowflake = toUser.Id
            };

            Context.UnverifyLog.Add(entity);
            Context.SaveChanges();
        }

        public List<UnverifyLog> GetOperationsLog(ulong? guildID, ulong? fromUserID, ulong? toUserID, UnverifyLogOperation? operation,
            DateTime? from, DateTime? to, int limit)
        {
            var query = Context.UnverifyLog.AsQueryable();

            if (guildID != null)
                query = query.Where(o => o.GuildID == guildID.Value.ToString());

            if (fromUserID != null)
                query = query.Where(o => o.FromUserID == fromUserID.Value.ToString());

            if (toUserID != null)
                query = query.Where(o => o.DestUserID == toUserID.Value.ToString());

            if (operation != null)
                query = query.Where(o => o.Operation == operation.Value);

            if (from != null)
                query = query.Where(o => o.DateTime >= from);

            if (to != null)
                query = query.Where(o => o.DateTime < to);

            return query
                .OrderByDescending(o => o.DateTime)
                .Take(limit)
                .ToList();
        }
    }
}
