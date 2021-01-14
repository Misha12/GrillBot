using Discord;
using Discord.Net;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Unverify;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Initiable;
using Grillbot.Services.Unverify.Models;
using Grillbot.Services.Unverify.Models.Log;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyService : IInitiable, IBackgroundTaskObserver
    {
        private UnverifyChecker Checker { get; }
        private UnverifyProfileGenerator UnverifyProfileGenerator { get; }
        public UnverifyLogger UnverifyLogger { get; }
        private UnverifyMessageGenerator MessageGenerator { get; }
        private UnverifyTimeParser TimeParser { get; }
        private BotState BotState { get; }
        private DiscordSocketClient DiscordClient { get; }
        private BotLoggingService Logger { get; }
        private ILogger<UnverifyService> AppLogger { get; }
        private IGrillBotRepository GrillBotRepository { get; }
        private BackgroundTaskQueue Queue { get; }

        public UnverifyService(UnverifyChecker checker, UnverifyProfileGenerator profileGenerator, UnverifyLogger logger,
            UnverifyMessageGenerator messageGenerator, UnverifyTimeParser timeParser, BotState botState, DiscordSocketClient discord,
            BotLoggingService loggingService, ILogger<UnverifyService> appLogger, IGrillBotRepository grillBotRepository,
            BackgroundTaskQueue queue)
        {
            Checker = checker;
            UnverifyProfileGenerator = profileGenerator;
            UnverifyLogger = logger;
            MessageGenerator = messageGenerator;
            TimeParser = timeParser;
            BotState = botState;
            DiscordClient = discord;
            Logger = loggingService;
            AppLogger = appLogger;
            GrillBotRepository = grillBotRepository;
            Queue = queue;
        }

        public async Task<List<string>> SetUnverifyAsync(List<SocketUser> users, string time, string data, SocketGuild guild, SocketUser fromUser)
        {
            var unverifyConfig = await GetUnverifyConfigAsync(guild);
            var messages = new List<string>();

            await guild.SyncGuildAsync();

            var mutedRole = guild.GetRole(unverifyConfig.MutedRoleID);

            foreach (var user in users)
            {
                var message = await SetUnverifyAsync(user, time, data, guild, fromUser, false, null, mutedRole, unverifyConfig);
                messages.Add(message);
            }

            return messages;
        }

        public async Task<string> SetUnverifyAsync(SocketUser user, string time, string data, SocketGuild guild, SocketUser fromUser, bool selfUnverify,
            List<string> toKeep)
        {
            var unverifyConfig = await GetUnverifyConfigAsync(guild);

            await guild.SyncGuildAsync();

            var mutedRole = guild.GetRole(unverifyConfig.MutedRoleID);
            return await SetUnverifyAsync(user, time, data, guild, fromUser, selfUnverify, toKeep, mutedRole, unverifyConfig);
        }

        private async Task<string> SetUnverifyAsync(SocketUser socketUser, string time, string data, SocketGuild guild, SocketUser fromUser, bool selfUnverify,
            List<string> toKeep, SocketRole mutedRole, UnverifyConfig unverifyConfig)
        {
            var user = await guild.GetUserFromGuildAsync(socketUser.Id);
            await Checker.ValidateAsync(user, guild, selfUnverify, unverifyConfig);

            var profile = await UnverifyProfileGenerator.CreateProfileAsync(user, guild, time, data, selfUnverify, toKeep, mutedRole);

            UnverifyLog unverifyLogEntity;
            if (selfUnverify)
                unverifyLogEntity = await UnverifyLogger.LogSelfUnverifyAsync(profile, guild);
            else
                unverifyLogEntity = await UnverifyLogger.LogUnverifyAsync(profile, guild, fromUser);

            try
            {
                if (mutedRole != null)
                    await user.SetRoleAsync(mutedRole);

                await user.RemoveRolesAsync(profile.RolesToRemove);

                foreach (var channelOverride in profile.ChannelsToRemove)
                {
                    var channel = guild.GetChannel(channelOverride.ChannelID);
                    await channel?.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Deny));
                }

                var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.Unverify);

                userEntity.Unverify = new Database.Entity.Unverify.Unverify()
                {
                    DeserializedChannels = profile.ChannelsToRemove.ConvertAll(o => new ChannelOverride(o.ChannelID, o.Perms)),
                    DeserializedRoles = profile.RolesToRemove.ConvertAll(o => o.Id),
                    EndDateTime = profile.EndDateTime,
                    Reason = profile.Reason,
                    StartDateTime = profile.StartDateTime,
                    SetLogOperation = unverifyLogEntity
                };

                await GrillBotRepository.CommitAsync();
                Queue.Add(new UnverifyBackgroundTask(guild.Id, user.Id, profile.EndDateTime));

                var pmMessage = MessageGenerator.CreateUnverifyPMMessage(profile, guild);
                await user.SendPrivateMessageAsync(pmMessage);

                return MessageGenerator.CreateUnverifyMessageToChannel(profile);
            }
            catch (Exception ex)
            {
                if (mutedRole != null)
                    await user.RemoveRoleAsync(mutedRole);

                await user.AddRolesAsync(profile.RolesToRemove);

                foreach (var channelOverride in profile.ChannelsToRemove)
                {
                    var channel = guild.GetChannel(channelOverride.ChannelID);

                    if (channel != null)
                        await channel.AddPermissionOverwriteAsync(user, channelOverride.Perms);
                }

                var errorMessage = new LogMessage(LogSeverity.Warning, nameof(UnverifyService), "An error occured when unverify removing access.", ex);
                await Logger.OnLogAsync(errorMessage);

                return MessageGenerator.CreateUnverifyFailedToChannel(user);
            }
        }

        private async Task<UnverifyConfig> GetUnverifyConfigAsync(SocketGuild guild)
        {
            var config = await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "unverify", null);
            return config.GetData<UnverifyConfig>();
        }

        public async Task<string> UpdateUnverifyAsync(SocketGuildUser user, SocketGuild guild, string time, SocketUser fromUser)
        {
            var task = Queue.Get<UnverifyBackgroundTask>(o => o.GuildId == guild.Id && o.UserId == user.Id);

            if (task == null)
                throw new NotFoundException("Aktualizace času nelze pro hledaného uživatele provést. Unverify nenalezeno.");

            if (task.CanProcess() || (task.At - DateTime.Now).TotalSeconds < 30.0D)
                throw new ValidationException("Aktualizace data a času již není možná. Vypršel čas, nebo zbývá méně, než půl minuty.");

            var endDateTime = TimeParser.Parse(time, minimumMinutes: 10);
            await UnverifyLogger.LogUpdateAsync(DateTime.Now, endDateTime, guild, fromUser, user);

            var userEntity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.Unverify);

            userEntity.Unverify.EndDateTime = endDateTime;
            userEntity.Unverify.StartDateTime = DateTime.Now;
            await GrillBotRepository.CommitAsync();

            task.At = endDateTime;

            var pmMessage = MessageGenerator.CreateUpdatePMMessage(guild, endDateTime);
            await user.SendPrivateMessageAsync(pmMessage);

            return MessageGenerator.CreateUpdateChannelMessage(user, endDateTime);
        }

        public async Task<List<UnverifyInfo>> GetCurrentUnverifies(SocketGuild guild)
        {
            var usersWithUnverify = GrillBotRepository.UsersRepository.GetUsersWithUnverify(guild.Id);

            var result = new List<UnverifyInfo>();

            foreach (var userEntity in usersWithUnverify)
            {
                var user = await guild.GetUserFromGuildAsync(userEntity.UserIDSnowflake);
                var logData = userEntity.Unverify.SetLogOperation.Json.ToObject<UnverifyLogSet>();

                var profile = new UnverifyUserProfile()
                {
                    Reason = userEntity.Unverify.Reason,
                    EndDateTime = userEntity.Unverify.EndDateTime,
                    StartDateTime = userEntity.Unverify.StartDateTime,
                    DestinationUser = user,
                    RolesToKeep = logData.RolesToKeep.Select(o => guild.GetRole(o)).Where(o => o != null).ToList(),
                    RolesToRemove = logData.RolesToRemove.Select(o => guild.GetRole(o)).Where(o => o != null).ToList(),
                    IsSelfUnverify = userEntity.Unverify.SetLogOperation.Operation == UnverifyLogOperation.Selfunverify
                };

                profile.ChannelsToKeep = logData.ChannelsToKeep.Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelID), new OverwritePermissions(o.AllowValue, o.DenyValue)))
                    .Where(o => o.Channel != null).ToList();

                profile.ChannelsToRemove = logData.ChannelsToRemove.Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelID), new OverwritePermissions(o.AllowValue, o.DenyValue)))
                    .Where(o => o.Channel != null).ToList();

                result.Add(new UnverifyInfo()
                {
                    ID = userEntity.ID,
                    Profile = profile
                });
            }

            return result;
        }

        public async Task AutoUnverifyRemoveAsync(ulong guildID, ulong userID)
        {
            try
            {
                var unverify = await GrillBotRepository.UnverifyRepository.FindUnverifyByUser(guildID, userID);

                if (unverify == null)
                    return;

                var guild = DiscordClient.GetGuild(guildID);

                if (guild == null)
                {
                    GrillBotRepository.Remove(unverify);
                    await GrillBotRepository.CommitAsync();
                    return;
                }

                var user = await guild.GetUserFromGuildAsync(userID);

                if (user == null)
                {
                    GrillBotRepository.Remove(unverify);
                    await GrillBotRepository.CommitAsync();
                    return;
                }

                await RemoveUnverifyAsync(guild, user, DiscordClient.CurrentUser, true);
            }
            catch (Exception ex)
            {
                var message = new LogMessage(LogSeverity.Error, nameof(UnverifyService), "An error occured when unverify returning access.", ex);
                await Logger.OnLogAsync(message);
            }
        }

        public async Task<string> RemoveUnverifyAsync(SocketGuild guild, SocketGuildUser user, SocketUser fromUser, bool isAuto = false)
        {
            try
            {
                BotState.CurrentReturningUnverifyFor.Add(user);

                var userEntity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.Unverify);

                if (userEntity?.Unverify == null)
                    return MessageGenerator.CreateRemoveAccessUnverifyNotFound(user);

                var unverifyConfig = (await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "unverify", null, false))?.GetData<UnverifyConfig>();
                var mutedRole = unverifyConfig == null ? null : guild.GetRole(unverifyConfig.MutedRoleID);

                var rolesToReturn = userEntity.Unverify.DeserializedRoles.Where(o => !user.Roles.Any(x => x.Id == o))
                    .Select(o => guild.GetRole(o)).Where(role => role != null).ToList();

                var channelsToReturn = userEntity.Unverify.DeserializedChannels
                    .Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelIdSnowflake), o.GetPermissions()))
                    .Where(o => o.Channel != null).ToList();

                if (isAuto)
                    await UnverifyLogger.LogAutoRemoveAsync(rolesToReturn, channelsToReturn, user, guild);
                else
                    await UnverifyLogger.LogRemoveAsync(rolesToReturn, channelsToReturn, guild, user, fromUser);

                foreach (var channel in channelsToReturn)
                {
                    if (channel.Channel is SocketGuildChannel socketGuildChannel)
                    {
                        try
                        {
                            await socketGuildChannel.AddPermissionOverwriteAsync(user, channel.Perms);
                        }
                        catch (HttpException ex)
                        {
                            var message = new LogMessage(LogSeverity.Error, nameof(UnverifyService), $"An error occured when unverify returning access to channel {channel.Channel.Name} for user {user.GetFullName()}", ex);
                            await Logger.OnLogAsync(message);
                        }
                    }
                }

                await user.AddRolesAsync(rolesToReturn);

                if (mutedRole != null)
                    await user.RemoveRoleAsync(mutedRole);

                userEntity.Unverify = null;
                await GrillBotRepository.CommitAsync();
                Queue.TryRemove<UnverifyBackgroundTask>(o => o.GuildId == guild.Id && o.UserId == user.Id);

                if (!isAuto)
                {
                    var message = MessageGenerator.CreateRemoveAccessManuallyPMMessage(guild);
                    await user.SendPrivateMessageAsync(message);
                }

                return MessageGenerator.CreateRemoveAccessManuallyToChannel(user);
            }
            catch (Exception ex)
            {
                if (!isAuto)
                    throw;

                var message = new LogMessage(LogSeverity.Error, nameof(UnverifyService), "An error occured when unverify returning access.", ex);
                await Logger.OnLogAsync(message);
                return MessageGenerator.CreateRemoveAccessManuallyFailed(user, ex);
            }
            finally
            {
                BotState.CurrentReturningUnverifyFor.RemoveAll(o => o.Id == user.Id);
            }
        }

        public async Task RemoveUnverifyFromWebAsync(long userId, SocketGuildUser fromUser)
        {
            var unverify = await GrillBotRepository.UnverifyRepository.FindUnverifyByID(userId);

            if (unverify == null)
                return;

            var guild = DiscordClient.GetGuild(unverify.User.GuildIDSnowflake);

            if (guild == null)
            {
                GrillBotRepository.Remove(unverify);
                await GrillBotRepository.CommitAsync();
                Queue.TryRemove<UnverifyBackgroundTask>(o => o.GuildId == unverify.User.GuildIDSnowflake && o.UserId == unverify.User.UserIDSnowflake);
                return;
            }

            var user = await guild.GetUserFromGuildAsync(unverify.User.UserIDSnowflake);

            if (user == null)
            {
                GrillBotRepository.Remove(unverify);
                await GrillBotRepository.CommitAsync();
                Queue.TryRemove<UnverifyBackgroundTask>(o => o.GuildId == guild.Id && o.UserId == unverify.User.UserIDSnowflake);
                return;
            }

            await RemoveUnverifyAsync(guild, user, fromUser);
            Queue.TryRemove<UnverifyBackgroundTask>(o => o.GuildId == guild.Id && o.UserId == user.Id);
        }

        public async Task OnUserLeftGuildAsync(SocketGuildUser user)
        {
            Queue.TryRemove<UnverifyBackgroundTask>(o => o.GuildId == user.Guild.Id && o.UserId == user.Id);

            var unverify = await GrillBotRepository.UnverifyRepository.FindUnverifyByUser(user.Guild.Id, user.Id);

            if (unverify == null)
                return;

            GrillBotRepository.Remove(unverify);
            await GrillBotRepository.CommitAsync();
        }

        public async Task RecoverToStateAsync(long id, SocketGuildUser fromUser)
        {
            var record = await GrillBotRepository.UnverifyRepository.FindLogItemByIDAsync(id);

            if (record == null)
                throw new NotFoundException($"Záznam o unverify s ID {id} nebyl nalezen.");

            if (record.Operation != UnverifyLogOperation.Unverify && record.Operation != UnverifyLogOperation.Selfunverify)
                throw new ValidationException("Obnovení lze provést pouze přes operace Unverify a SelfUnverify");

            if (record.ToUser.Unverify != null)
                throw new ValidationException("Nelze provést obnovení stavu před unverify uživateli, který má unverify.");

            var guild = DiscordClient.GetGuild(record.ToUser.GuildIDSnowflake);
            if (guild == null)
                throw new NotFoundException("Nelze najít server, na kterém bylo uživateli uděleno unverify.");

            var user = await guild.GetUserFromGuildAsync(record.ToUser.UserIDSnowflake);
            if (user == null)
                throw new NotFoundException($"Nelze vyhledat uživatele na serveru {guild.Name}");

            var unverifyConfig = (await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "unverify", null, false))?.GetData<UnverifyConfig>();
            var mutedRole = unverifyConfig == null ? null : guild.GetRole(unverifyConfig.MutedRoleID);

            var data = record.Json.ToObject<UnverifyLogSet>();

            var rolesToReturn = data.RolesToRemove.Where(o => !user.Roles.Any(x => x.Id == o))
                .Select(o => guild.GetRole(o)).Where(role => role != null).ToList();

            var channelsToReturn = data.ChannelsToRemove
                .Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelID), new OverwritePermissions(o.AllowValue, o.DenyValue)))
                .Where(o => o.Channel is SocketGuildChannel)
                .Where(o =>
                {
                    var perms = ((SocketGuildChannel)o.Channel).GetPermissionOverwrite(user);
                    return perms != null && (perms.Value.AllowValue != o.AllowValue || perms.Value.DenyValue != o.DenyValue);
                })
                .ToList();

            await UnverifyLogger.LogRecoverAsync(rolesToReturn, channelsToReturn, guild, user, fromUser);

            if (rolesToReturn.Count > 0)
                await user.AddRolesAsync(rolesToReturn);

            if (channelsToReturn.Count > 0)
            {
                foreach (var channel in channelsToReturn)
                {
                    if (channel.Channel is SocketGuildChannel socketGuildChannel)
                    {
                        await socketGuildChannel.AddPermissionOverwriteAsync(user, channel.Perms);
                    }
                }
            }

            if (mutedRole != null)
                await user.RemoveRoleAsync(mutedRole);
        }

        #region Imunity

        public async Task SetImunityAsync(IGuild guild, IUser user, string groupName)
        {
            var dbUser = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.None);

            dbUser.UnverifyImunityGroup = groupName;
            await GrillBotRepository.CommitAsync();
        }

        public async Task<Dictionary<string, int>> GetImunityGroupsAsync(IGuild guild)
        {
            var data = await GrillBotRepository.UsersRepository.GetUsersWithUnverifyImunity(guild.Id)
                .GroupBy(o => o.UnverifyImunityGroup)
                .Select(o => new
                {
                    GroupName = o.Key,
                    Count = o.Count()
                }).ToListAsync();

            return data.ToDictionary(o => o.GroupName, o => o.Count);
        }

        public async Task RemoveImunityAsync(IGuild guild, IUser user)
        {
            var dbUser = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (string.IsNullOrEmpty(dbUser?.UnverifyImunityGroup))
                throw new ValidationException($"Uživatel **{user.GetFullName()}** neměl imunitu vůči unverify.");

            dbUser.UnverifyImunityGroup = null;
            await GrillBotRepository.CommitAsync();
        }

        public async Task<List<string>> GetUnverifyGroupUsersAsync(SocketGuild guild, string groupName)
        {
            var users = await GrillBotRepository.UsersRepository.GetUsersWithUnverifyImunity(guild.Id)
                .Where(o => o.UnverifyImunityGroup == groupName)
                .Select(o => o.UserID)
                .ToListAsync();

            var usernames = new List<string>();

            foreach (var userID in users.Select(o => Convert.ToUInt64(o)))
            {
                var user = await guild.GetUserFromGuildAsync(userID);

                if (user != null)
                    usernames.Add($"> {user.GetFullName()}");
            }

            return usernames.OrderByDescending(o => o).ToList();
        }

        #endregion

        #region SelfUnverify

        public async Task AddSelfunverifyDefinitionsAsync(SocketGuild guild, string group, string[] values)
        {
            var config = await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "selfunverify", null, false);
            var jsonData = config.GetData<SelfUnverifyConfig>();

            if (!jsonData.RolesToKeep.ContainsKey(group))
                jsonData.RolesToKeep.Add(group, new List<string>());

            jsonData.RolesToKeep[group].AddRange(values);
            jsonData.RolesToKeep[group] = jsonData.RolesToKeep[group].Distinct().ToList();

            config.Config = JObject.FromObject(jsonData);
            await GrillBotRepository.CommitAsync();
        }

        public async Task<Tuple<List<string>, List<string>>> RemoveSelfunverifyDefinitions(SocketGuild guild, string group, string[] values)
        {
            var config = await GrillBotRepository.ConfigRepository.FindConfigAsync(guild.Id, "selfunverify", null, false);
            var jsonData = config.GetData<SelfUnverifyConfig>();

            if (!jsonData.RolesToKeep.ContainsKey(group))
                throw new ValidationException($"Skupina `{group}` neexistuje.");

            var exists = values.Where(o => jsonData.RolesToKeep[group].Contains(o)).ToList();
            var notExists = values.Where(o => !jsonData.RolesToKeep[group].Contains(o)).ToList();

            foreach (var item in exists)
            {
                jsonData.RolesToKeep[group].Remove(item);
            }

            if (jsonData.RolesToKeep[group].Count == 0)
                jsonData.RolesToKeep.Remove(group);

            config.Config = JObject.FromObject(jsonData);
            await GrillBotRepository.CommitAsync();
            return Tuple.Create(exists, notExists);
        }

        #endregion

        public void Init() { }

        public async Task InitAsync()
        {
            foreach (var guild in DiscordClient.Guilds)
            {
                var unverifies = await GrillBotRepository.UsersRepository.GetUsersWithUnverify(guild.Id).ToListAsync();
                if (unverifies.Count == 0) continue;

                foreach (var unverify in unverifies)
                {
                    var task = new UnverifyBackgroundTask(guild.Id, unverify.UserIDSnowflake, unverify.Unverify.EndDateTime);
                    Queue.Add(task);
                }
            }

            AppLogger.LogInformation("Unverify loaded.");
        }

        public async Task TriggerBackgroundTaskAsync(object data)
        {
            if (data is not UnverifyBackgroundTask task)
                return;

            await AutoUnverifyRemoveAsync(task.GuildId, task.UserId);
        }
    }
}
