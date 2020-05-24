using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.UnverifyLog;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models.TempUnverify.Admin;
using Grillbot.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyLogService : IDisposable
    {
        private TempUnverifyRepository Repository { get; }
        private DiscordSocketClient DiscordClient { get; }

        public TempUnverifyLogService(TempUnverifyRepository repository, DiscordSocketClient discordClient)
        {
            Repository = repository;
            DiscordClient = discordClient;
        }

        public void LogSet(List<ChannelOverride> overrides, List<ulong> roleIDs, int unverifyTime, string reason, IUser toUser, IUser fromUser,
            IGuild guild, bool isSelfUnverify, string[] subjects)
        {
            var data = new UnverifyLogSet()
            {
                Overrides = overrides,
                Reason = reason,
                Roles = roleIDs,
                StartAt = DateTime.Now,
                TimeFor = unverifyTime.ToString(),
                IsSelfUnverify = isSelfUnverify,
                Subjects = subjects?.ToList() ?? new List<string>()
            };

            Save(UnverifyLogOperation.Set, fromUser, toUser, guild, data);
        }

        public void LogAutoRemove(TempUnverifyItem item, IUser toUser, IGuild guild)
        {
            var data = new UnverifyLogRemove()
            {
                Overrides = item.DeserializedChannelOverrides,
                Roles = item.DeserializedRolesToReturn
            };

            Save(UnverifyLogOperation.AutoRemove, DiscordClient.CurrentUser, toUser, guild, data);
        }

        public void LogRemove(TempUnverifyItem item, IUser toUser, IUser fromUser, IGuild guild)
        {
            var data = new UnverifyLogRemove()
            {
                Overrides = item.DeserializedChannelOverrides,
                Roles = item.DeserializedRolesToReturn
            };

            Save(UnverifyLogOperation.Remove, fromUser, toUser, guild, data);
        }

        public void LogUpdate(int unverifyTime, IUser fromUser, IUser toUser, IGuild guild)
        {
            var data = new UnverifyLogUpdate()
            {
                TimeFor = unverifyTime.ToString()
            };

            Save(UnverifyLogOperation.Update, fromUser, toUser, guild, data);
        }

        private void Save(UnverifyLogOperation operation, IUser fromUser, IUser toUser, IGuild guild, object data)
        {
            Repository.LogOperation(operation, fromUser, guild, toUser, data);
        }

        public void Dispose()
        {
            Repository.Dispose();
        }

        public async Task<List<UnverifyAuditItem>> GetAuditLogAsync(UnverifyAuditFilterRequest filter)
        {
            var data = Repository.GetOperationsLog(filter.GuildID, filter.FromUserID, filter.DestUserID, filter.Operation,
                filter.DateTimeFrom, filter.DateTimeTo, 200);

            var result = new List<UnverifyAuditItem>();

            foreach(var item in data)
            {
                var auditItem = new UnverifyAuditItem(item, DiscordClient);

                if (CanIgnoreSelfUnverifyItem(auditItem, filter.IgnoreSelfUnverify))
                    continue;

                auditItem.FromUser = await auditItem.Guild.GetUserFromGuildAsync(item.FromUserIDSnowflake);
                auditItem.ToUser = await auditItem.Guild.GetUserFromGuildAsync(item.DestUserIDSnowflake);
                result.Add(auditItem);
            }

            return result;
        }

        public async Task<List<UserUnverifyHistoryItem>> GetUnverifyHistoryOfUserAsync(DBDiscordUser user)
        {
            var data = Repository.GetHistoryOfUser(user.GuildIDSnowflake, user.UserIDSnowflake);

            var result = new List<UserUnverifyHistoryItem>();

            foreach(var item in data)
            {
                var guild = DiscordClient.GetGuild(item.GuildIDSnowflake);

                var historyItem = new UserUnverifyHistoryItem(item)
                {
                    FromUser = (await guild.GetUserFromGuildAsync(item.FromUserIDSnowflake))?.GetFullName()
                };

                result.Add(historyItem);
            }

            return result;
        }

        public bool CanIgnoreSelfUnverifyItem(UnverifyAuditItem item, bool ignoreSelfUnverify)
        {
            return ignoreSelfUnverify && item.IsSelfUnverify();
        }
    }
}
