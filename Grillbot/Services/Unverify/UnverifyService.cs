using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyService : IDisposable
    {
        private UnverifyChecker Checker { get; }
        private UnverifyProfileGenerator UnverifyProfileGenerator { get; }
        private UnverifyLogger UnverifyLogger { get; }
        private UnverifyMessageGenerator MessageGenerator { get; }
        private ConfigRepository ConfigRepository { get; }
        private UsersRepository UsersRepository { get; }
        private UnverifyTimeParser TimeParser { get; }
        private BotState BotState { get; }

        public UnverifyService(UnverifyChecker checker, UnverifyProfileGenerator profileGenerator, UnverifyLogger logger,
            UnverifyMessageGenerator messageGenerator, ConfigRepository configRepository, UsersRepository usersRepository,
            UnverifyTimeParser timeParser, BotState botState)
        {
            Checker = checker;
            UnverifyProfileGenerator = profileGenerator;
            UnverifyLogger = logger;
            MessageGenerator = messageGenerator;
            ConfigRepository = configRepository;
            UsersRepository = usersRepository;
            TimeParser = timeParser;
            BotState = botState;
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
            Checker.Validate(user, guild, selfUnverify);

            var profile = await UnverifyProfileGenerator.CreateProfileAsync(user, guild, time, data, selfUnverify, toKeep, mutedRole);

            if (selfUnverify)
                UnverifyLogger.LogSelfUnverify(profile, guild);
            else
                UnverifyLogger.LogUnverify(profile, guild, fromUser);

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
                StartDateTime = profile.StartDateTime
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

        public void Dispose()
        {
            Checker.Dispose();
            UnverifyProfileGenerator.Dispose();
            UnverifyLogger.Dispose();
            ConfigRepository.Dispose();
            UsersRepository.Dispose();
        }
    }
}
