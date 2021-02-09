using Discord;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Unverify;
using Grillbot.Services.Unverify.Models;
using Grillbot.Services.Unverify.Models.Log;
using Grillbot.Services.Unverify.WebAdmin;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyLogger
    {
        private DiscordSocketClient Discord { get; }
        private UnverifyModelConverter Converter { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public UnverifyLogger(DiscordSocketClient discord, IGrillBotRepository grillBotRepository, UnverifyModelConverter converter)
        {
            GrillBotRepository = grillBotRepository;
            Discord = discord;
            Converter = converter;
        }

        public async Task<UnverifyLog> LogUnverifyAsync(UnverifyUserProfile profile, IGuild guild, IUser fromUser)
        {
            var data = UnverifyLogSet.FromProfile(profile);

            var fromUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, fromUser.Id, UsersIncludes.None);
            var toUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, profile.DestinationUser.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();

            GrillBotRepository.Detach(fromUserEntity);
            GrillBotRepository.Detach(toUserEntity);

            return await SaveLogOperationAsync(UnverifyLogOperation.Unverify, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public async Task<UnverifyLog> LogSelfUnverifyAsync(UnverifyUserProfile profile, IGuild guild)
        {
            var data = UnverifyLogSet.FromProfile(profile);

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, profile.DestinationUser.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();

            GrillBotRepository.Detach(userEntity);

            return await SaveLogOperationAsync(UnverifyLogOperation.Selfunverify, data.ToJObject(), userEntity.ID, userEntity.ID);
        }

        public async Task LogAutoRemoveAsync(List<SocketRole> returnedRoles, List<ChannelOverwrite> returnedChannels, IUser toUser, IGuild guild)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverrides = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, toUser.Id, UsersIncludes.None);
            var botUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, Discord.CurrentUser.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();
            GrillBotRepository.Detach(userEntity);
            GrillBotRepository.Detach(botUserEntity);

            await SaveLogOperationAsync(UnverifyLogOperation.Autoremove, data.ToJObject(), botUserEntity.ID, userEntity.ID);
        }

        public async Task LogRemoveAsync(List<SocketRole> returnedRoles, List<ChannelOverwrite> returnedChannels, IGuild guild, IUser toUser, IUser fromUser)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverrides = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            var toUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, toUser.Id, UsersIncludes.None);
            var fromUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, fromUser.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();

            GrillBotRepository.Detach(toUserEntity);
            GrillBotRepository.Detach(fromUserEntity);

            await SaveLogOperationAsync(UnverifyLogOperation.Remove, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public async Task LogUpdateAsync(DateTime startDateTime, DateTime endDateTime, IGuild guild, IUser fromUser, IUser toUser)
        {
            var data = new UnverifyLogUpdate()
            {
                EndDateTime = endDateTime,
                StartDateTime = startDateTime
            };

            var toUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, toUser.Id, UsersIncludes.None);
            var fromUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, fromUser.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();

            GrillBotRepository.Detach(toUserEntity);
            GrillBotRepository.Detach(fromUserEntity);

            await SaveLogOperationAsync(UnverifyLogOperation.Update, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public async Task LogRecoverAsync(List<SocketRole> returnedRoles, List<ChannelOverwrite> returnedChannels, IGuild guild, IUser toUser, IUser fromUser)
        {
            var data = new UnverifyLogRemove()
            {
                ReturnedOverrides = returnedChannels,
                ReturnedRoles = returnedRoles.ConvertAll(o => o.Id)
            };

            var toUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, toUser.Id, UsersIncludes.None);
            var fromUserEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, fromUser.Id, UsersIncludes.None);
            await GrillBotRepository.CommitAsync();

            GrillBotRepository.Detach(toUserEntity);
            GrillBotRepository.Detach(fromUserEntity);

            await SaveLogOperationAsync(UnverifyLogOperation.Recover, data.ToJObject(), fromUserEntity.ID, toUserEntity.ID);
        }

        public async Task<List<UnverifyLogItem>> GetLogsAsync(UnverifyAuditFilterFormData formData)
        {
            var filter = await Converter.ConvertAuditFilter(formData);
            var data = await GrillBotRepository.UnverifyRepository.GetLogs(filter).ToListAsync();

            return data.ConvertAll(o => new UnverifyLogItem(o, Discord));
        }

        public async Task<PaginationInfo> CreatePaginationInfo(UnverifyAuditFilterFormData formData)
        {
            var filter = await Converter.ConvertAuditFilter(formData);
            var logsCount = await GrillBotRepository.UnverifyRepository.GetLogs(filter, true).CountAsync();

            return new PaginationInfo(filter.Skip, formData.Page, logsCount);
        }

        public async Task<UnverifyLog> SaveLogOperationAsync(UnverifyLogOperation operation, JObject jsonData, long fromUserID, long toUserID)
        {
            var entity = new UnverifyLog()
            {
                CreatedAt = DateTime.Now,
                FromUserID = fromUserID,
                Json = jsonData,
                Operation = operation,
                ToUserID = toUserID
            };

            await GrillBotRepository.AddAsync(entity);
            await GrillBotRepository.CommitAsync();

            return entity;
        }

        public async Task<Dictionary<IUser, Tuple<int, int>>> GetUnverifyStatisticsAsync(SocketGuild guild)
        {
            var stats = await GrillBotRepository.UnverifyRepository.GetUnverifyStatisticsAsync(guild.Id);
            var result = new Dictionary<IUser, Tuple<int, int>>();

            foreach(var item in stats.OrderByDescending(o => o.Value.Item1 + o.Value.Item2))
            {
                var user = await guild.GetUserFromGuildAsync(item.Key);

                if (user != null)
                    result.Add(user, item.Value);
            }

            return result;
        }
    }
}
