using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Models;
using Grillbot.Models.Unverify;
using Grillbot.Services.Unverify.Models;
using Grillbot.Services.Unverify.Models.Log;
using Grillbot.Services.Unverify.WebAdmin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyLogger : IDisposable
    {
        private UsersRepository UsersRepository { get; }
        private DiscordSocketClient Discord { get; }
        private UnverifyRepository UnverifyRepository { get; }
        private UnverifyModelConverter Converter { get; }

        public UnverifyLogger(UsersRepository usersRepository, DiscordSocketClient discord, UnverifyRepository unverifyRepository,
            UnverifyModelConverter converter)
        {
            UsersRepository = usersRepository;
            Discord = discord;
            UnverifyRepository = unverifyRepository;
            Converter = converter;
        }

        public UnverifyLog LogUnverify(UnverifyUserProfile profile, IGuild guild, IUser fromUser)
        {
            var data = UnverifyLogSet.FromProfile(profile);

            var fromUserEntity = UsersRepository.GetOrCreateUser(guild.Id, fromUser.Id, UsersIncludes.None);
            var toUserEntity = UsersRepository.GetOrCreateUser(guild.Id, profile.DestinationUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            return UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Unverify, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public UnverifyLog LogSelfUnverify(UnverifyUserProfile profile, IGuild guild)
        {
            var data = UnverifyLogSet.FromProfile(profile);

            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, profile.DestinationUser.Id, UsersIncludes.None);
            UsersRepository.SaveChangesIfAny();

            return UnverifyRepository.SaveLogOperation(UnverifyLogOperation.Selfunverify, data.ToJObject(), userEntity.ID, userEntity.ID);
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

        public async Task<List<UnverifyLogItem>> GetLogsAsync(UnverifyAuditFilterFormData formData)
        {
            var filter = await Converter.ConvertAuditFilter(formData);
            var data = await UnverifyRepository.GetLogs(filter).ToListAsync();

            return data.Select(o => new UnverifyLogItem(o, Discord)).ToList();
        }

        public async Task<PaginationInfo> CreatePaginationInfo(UnverifyAuditFilterFormData formData)
        {
            var filter = await Converter.ConvertAuditFilter(formData);
            var logsCount = await UnverifyRepository.GetLogs(filter, true).CountAsync();

            return new PaginationInfo()
            {
                CanPrev = filter.Skip != 0,
                CanNext = filter.Skip + filter.Take < logsCount,
                Page = formData.Page,
            };
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
            UnverifyRepository.Dispose();
            Converter.Dispose();
        }
    }
}
