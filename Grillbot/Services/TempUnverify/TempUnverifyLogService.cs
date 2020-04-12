using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.UnverifyLog;
using Grillbot.Database.Repository;
using System;
using System.Collections.Generic;

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

        public void LogSet(List<ChannelOverride> overrides, List<ulong> roleIDs, int unverifyTime, string reason, DateTime startAt, IUser toUser,
            IUser fromUser, IGuild guild)
        {
            var data = new UnverifyLogSet()
            {
                Overrides = overrides,
                Reason = reason,
                Roles = roleIDs,
                StartAt = startAt,
                TimeFor = unverifyTime.ToString()
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

        private void Save(UnverifyLogOperation operation, IUser fromUser, IUser toUser, IGuild guild, UnverifyLogDataBase data)
        {
            data.SetUser(toUser);
            Repository.LogOperation(operation, fromUser, guild, data);
        }

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}
