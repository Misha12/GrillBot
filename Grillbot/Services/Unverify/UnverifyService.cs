using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Unverify;
using Grillbot.Services.Initiable;
using Grillbot.Services.Unverify.Models;
using Grillbot.Services.Unverify.Models.Log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyService : IDisposable, IInitiable
    {
        private UnverifyChecker Checker { get; }
        private UnverifyProfileGenerator UnverifyProfileGenerator { get; }
        public UnverifyLogger UnverifyLogger { get; }
        private UnverifyMessageGenerator MessageGenerator { get; }
        private ConfigRepository ConfigRepository { get; }
        private UsersRepository UsersRepository { get; }
        private UnverifyTimeParser TimeParser { get; }
        private BotState BotState { get; }
        private DiscordSocketClient DiscordClient { get; }
        private BotLoggingService Logger { get; }
        private UnverifyRepository UnverifyRepository { get; }
        private ILogger<UnverifyService> AppLogger { get; }

        public UnverifyService(UnverifyChecker checker, UnverifyProfileGenerator profileGenerator, UnverifyLogger logger,
            UnverifyMessageGenerator messageGenerator, ConfigRepository configRepository, UsersRepository usersRepository,
            UnverifyTimeParser timeParser, BotState botState, DiscordSocketClient discord, BotLoggingService loggingService,
            UnverifyRepository unverifyRepository, ILogger<UnverifyService> appLogger)
        {
            Checker = checker;
            UnverifyProfileGenerator = profileGenerator;
            UnverifyLogger = logger;
            MessageGenerator = messageGenerator;
            ConfigRepository = configRepository;
            UsersRepository = usersRepository;
            TimeParser = timeParser;
            BotState = botState;
            DiscordClient = discord;
            Logger = loggingService;
            UnverifyRepository = unverifyRepository;
            AppLogger = appLogger;
        }

        public async Task<List<string>> SetUnverifyAsync(List<SocketGuildUser> users, string time, string data, SocketGuild guild, SocketUser fromUser)
        {
            var unverifyConfig = GetUnverifyConfig(guild);
            var messages = new List<string>();

            await guild.SyncGuildAsync();

            var mutedRole = guild.GetRole(unverifyConfig.MutedRoleID);

            foreach (var user in users)
            {
                var message = await SetUnverifyAsync(user, time, data, guild, fromUser, false, null, mutedRole);
                messages.Add(message);
            }

            return messages;
        }

        public async Task<string> SetUnverifyAsync(SocketGuildUser user, string time, string data, SocketGuild guild, SocketUser fromUser, bool selfUnverify,
            List<string> toKeep)
        {
            var unverifyConfig = GetUnverifyConfig(guild);

            await guild.SyncGuildAsync();

            var mutedRole = guild.GetRole(unverifyConfig.MutedRoleID);

            return await SetUnverifyAsync(user, time, data, guild, fromUser, selfUnverify, toKeep, mutedRole);
        }

        private async Task<string> SetUnverifyAsync(SocketGuildUser user, string time, string data, SocketGuild guild, SocketUser fromUser, bool selfUnverify,
            List<string> toKeep, SocketRole mutedRole)
        {
            await Checker.ValidateAsync(user, guild, selfUnverify);

            var profile = await UnverifyProfileGenerator.CreateProfileAsync(user, guild, time, data, selfUnverify, toKeep, mutedRole);

            UnverifyLog unverifyLogEntity;
            if (selfUnverify)
                unverifyLogEntity = UnverifyLogger.LogSelfUnverify(profile, guild);
            else
                unverifyLogEntity = UnverifyLogger.LogUnverify(profile, guild, fromUser);

            if (mutedRole != null)
                await user.SetRoleAsync(mutedRole);

            await user.RemoveRolesAsync(profile.RolesToRemove);

            foreach (var channelOverride in profile.ChannelsToRemove)
            {
                var channel = guild.GetChannel(channelOverride.ChannelID);
                await channel?.AddPermissionOverwriteAsync(user, new OverwritePermissions(viewChannel: PermValue.Deny));
            }

            var userEntity = UsersRepository.GetOrCreateUser(guild.Id, user.Id, UsersIncludes.Unverify);

            userEntity.Unverify = new Database.Entity.Unverify.Unverify()
            {
                DeserializedChannels = profile.ChannelsToRemove.Select(o => new ChannelOverride(o.ChannelID, o.Perms)).ToList(),
                DeserializedRoles = profile.RolesToRemove.Select(o => o.Id).ToList(),
                EndDateTime = profile.EndDateTime,
                Reason = profile.Reason,
                StartDateTime = profile.StartDateTime,
                SetLogOperation = unverifyLogEntity
            };

            UsersRepository.SaveChanges();
            BotState.UnverifyCache.Add(CreateUnverifyCacheKey(guild, user), profile.EndDateTime);

            var pmMessage = MessageGenerator.CreateUnverifyPMMessage(profile, guild);
            await user.SendPrivateMessageAsync(pmMessage);

            return MessageGenerator.CreateUnverifyMessageToChannel(profile);
        }

        private UnverifyConfig GetUnverifyConfig(SocketGuild guild)
        {
            var config = ConfigRepository.FindConfig(guild.Id, "unverify", null);
            return config.GetData<UnverifyConfig>();
        }

        private string CreateUnverifyCacheKey(SocketGuild guild, SocketGuildUser user)
        {
            return $"{guild.Id}|{user.Id}";
        }

        public async Task<string> UpdateUnverifyAsync(SocketGuildUser user, SocketGuild guild, string time, SocketUser fromUser)
        {
            var cacheKey = CreateUnverifyCacheKey(guild, user);
            if (!BotState.UnverifyCache.ContainsKey(cacheKey))
                throw new NotFoundException("Aktualizace času nelze pro hledaného uživatele provést. Unverify nenalezeno.");

            if ((BotState.UnverifyCache[cacheKey] - DateTime.Now).TotalSeconds < 30.0D)
                throw new ValidationException("Aktualizace data a času již není možná. Zbývá méně, než půl minuty.");

            var endDateTime = TimeParser.Parse(time, minimumMinutes: 10);
            UnverifyLogger.LogUpdate(DateTime.Now, endDateTime, guild, fromUser, user);

            var userEntity = UsersRepository.GetUser(guild.Id, user.Id, UsersIncludes.Unverify);

            userEntity.Unverify.EndDateTime = endDateTime;
            userEntity.Unverify.StartDateTime = DateTime.Now;
            UsersRepository.SaveChanges();

            BotState.UnverifyCache[cacheKey] = endDateTime;

            var pmMessage = MessageGenerator.CreateUpdatePMMessage(guild, endDateTime);
            await user.SendPrivateMessageAsync(pmMessage);

            return MessageGenerator.CreateUpdateChannelMessage(user, endDateTime);
        }

        public async Task<List<UnverifyInfo>> GetCurrentUnverifies(SocketGuild guild)
        {
            var usersWithUnverify = UsersRepository.GetUsersWithUnverify(guild.Id);

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

        public async Task AutoUnverifyRemoveAsync(string guildID, string userID)
        {
            try
            {
                var guildIDSnowflake = Convert.ToUInt64(guildID);
                var userIDSnowflake = Convert.ToUInt64(userID);

                var guild = DiscordClient.GetGuild(guildIDSnowflake);

                if (guild == null)
                {
                    UnverifyRepository.RemoveUnverify(guildIDSnowflake, userIDSnowflake);
                    return;
                }

                var user = await guild.GetUserFromGuildAsync(userIDSnowflake);

                if (user == null)
                {
                    UnverifyRepository.RemoveUnverify(guildIDSnowflake, userIDSnowflake);
                    return;
                }

                await RemoveUnverifyAsync(guild, user, DiscordClient.CurrentUser, true);
            }
            catch (Exception ex)
            {
                var message = new LogMessage(LogSeverity.Error, nameof(UnverifyService), $"An error occured when unverify returning access.", ex);
                await Logger.OnLogAsync(message);
            }
        }

        public async Task<string> RemoveUnverifyAsync(SocketGuild guild, SocketGuildUser user, SocketUser fromUser, bool isAuto = false)
        {
            try
            {
                BotState.CurrentReturningUnverifyFor.Add(user);

                var userEntity = UsersRepository.GetUser(guild.Id, user.Id, UsersIncludes.Unverify);

                if (userEntity?.Unverify == null)
                    return MessageGenerator.CreateRemoveAccessUnverifyNotFound(user);

                var unverifyConfig = ConfigRepository.FindConfig(guild.Id, "unverify", null, false)?.GetData<UnverifyConfig>();
                var mutedRole = unverifyConfig == null ? null : guild.GetRole(unverifyConfig.MutedRoleID);

                var rolesToReturn = userEntity.Unverify.DeserializedRoles.Where(o => !user.Roles.Any(x => x.Id == o))
                    .Select(o => guild.GetRole(o)).Where(role => role != null).ToList();

                var channelsToReturn = userEntity.Unverify.DeserializedChannels
                    .Select(o => new ChannelOverwrite(guild.GetChannel(o.ChannelIdSnowflake), o.GetPermissions()))
                    .Where(o => o.Channel != null).ToList();

                if (isAuto)
                    UnverifyLogger.LogAutoRemove(rolesToReturn, channelsToReturn, user, guild);
                else
                    UnverifyLogger.LogRemove(rolesToReturn, channelsToReturn, guild, user, fromUser);

                var tasks = new List<Task>();

                tasks.AddRange(channelsToReturn.Select(o => (o.Channel as SocketGuildChannel)?.AddPermissionOverwriteAsync(user, o.Perms)).Where(o => o != null));
                tasks.Add(user.AddRolesAsync(rolesToReturn));

                if (mutedRole != null && user.Roles.Any(x => x == mutedRole))
                    tasks.Add(user.RemoveRoleAsync(mutedRole));

                await Task.WhenAll(tasks.ToArray());
                UnverifyRepository.RemoveUnverify(guild.Id, user.Id);

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

                var message = new LogMessage(LogSeverity.Error, nameof(UnverifyService), $"An error occured when unverify returning access.", ex);
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
            var unverify = UnverifyRepository.FindUnverifyByID(userId);

            if (unverify == null)
                return;

            var guild = DiscordClient.GetGuild(unverify.User.GuildIDSnowflake);

            if (guild == null)
            {
                UnverifyRepository.RemoveUnverify(unverify.User.GuildIDSnowflake, unverify.User.UserIDSnowflake);
                BotState.UnverifyCache.Remove($"{unverify.User.GuildID}|{unverify.User.UserID}");
                return;
            }

            var user = await guild.GetUserFromGuildAsync(unverify.User.UserIDSnowflake);

            if (user == null)
            {
                UnverifyRepository.RemoveUnverify(guild.Id, unverify.User.UserIDSnowflake);
                BotState.UnverifyCache.Remove($"{guild.Id}|{unverify.User.UserID}");
                return;
            }

            await RemoveUnverifyAsync(guild, user, fromUser);
            BotState.UnverifyCache.Remove(CreateUnverifyCacheKey(guild, user));
        }

        public void OnUserLeftGuild(SocketGuildUser user)
        {
            BotState.UnverifyCache.Remove(CreateUnverifyCacheKey(user.Guild, user));
            UnverifyRepository.RemoveUnverify(user.Guild.Id, user.Id);
        }

        public void Dispose()
        {
            Checker.Dispose();
            UnverifyProfileGenerator.Dispose();
            UnverifyLogger.Dispose();
            ConfigRepository.Dispose();
            UsersRepository.Dispose();
            UnverifyRepository.Dispose();
        }

        public void Init() { }

        public async Task InitAsync()
        {
            if (BotState.UnverifyCache.Count > 0)
                BotState.UnverifyCache.Clear();

            foreach (var guild in DiscordClient.Guilds)
            {
                var unverifies = UsersRepository.GetUsersWithUnverify(guild.Id).ToList();
                if (unverifies.Count == 0) continue;

                foreach (var unverify in unverifies)
                {
                    var user = await guild.GetUserFromGuildAsync(unverify.UserIDSnowflake);
                    var key = CreateUnverifyCacheKey(guild, user);

                    BotState.UnverifyCache.Add(key, unverify.Unverify.EndDateTime);
                }
            }

            AppLogger.LogInformation($"Unverify loaded. Loaded entities: {BotState.UnverifyCache.Count}");
        }
    }
}
