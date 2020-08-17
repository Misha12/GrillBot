using Discord;
using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Services.Unverify.Models;
using Grillbot.Services.Unverify.Models.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.Unverify
{
    public class UnverifyLogger : IDisposable
    {
        private UsersRepository UsersRepository { get; }
        private DiscordSocketClient Discord { get; }
        private UnverifyRepository UnverifyRepository { get; }

        public UnverifyLogger(UsersRepository usersRepository, DiscordSocketClient discord, UnverifyRepository unverifyRepository)
        {
            UsersRepository = usersRepository;
            Discord = discord;
            UnverifyRepository = unverifyRepository;
        }

        public void LogUnverify(UnverifyUserProfile profile, IGuild guild, IUser fromUser)
        {
            var data = UnverifyLogSet.FromProfile(profile);

            var fromUserEntity = UsersRepository.GetOrCreateUser(guild.Id, fromUser.Id, UsersIncludes.None);
            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, profile.DestinationUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Unverify, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public void LogSelfUnverify(UnverifyUserProfile profile, IGuild guild)
        {
            var data = UnverifyLogSet.FromProfile(profile);

            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, profile.DestinationUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Selfunverify, data.ToJObject(), userEntity.ID, userEntity.ID);
        }

        public void LogAutoRemove(List<SocketRole> returnedRoles, List<ChannelOverwrite> returnedChannels, IUser toUser, IGuild guild)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverrides = returnedChannels,
                ReturnedRoles = returnedRoles.Select(o => o.Id).ToList()
            };

            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, UsersIncludes.None);
            var botUserEntity = UsersRepository.GetOrCreateUser(guild.Id, Discord.CurrentUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Autoremove, data.ToJObject(), botUserEntity.ID, userEntity.ID);
        }

        public void LogRemove(List<SocketRole> returnedRoles, List<ChannelOverwrite> returnedChannels, IGuild guild, IUser toUser, IUser fromUser)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverrides = returnedChannels,
                ReturnedRoles = returnedRoles.Select(o => o.Id).ToList()
            };

            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, UsersIncludes.None);
            var fromUserEntity = UsersRepository.GetOrCreateUser(guild.Id, fromUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Remove, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public void LogUpdate(DateTime startDateTime, DateTime endDateTime, IGuild guild, IUser fromUser, IUser toUser)
        {
            var data = new UnverifyLogUpdate()
            {
                EndDateTime = endDateTime,
                StartDateTime = startDateTime
            };

            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, toUser.Id, UsersIncludes.None);
            var fromUserEntity = UsersRepository.GetOrCreateUser(guild.Id, fromUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Update, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
            UnverifyRepository.Dispose();
        }
    }
}
