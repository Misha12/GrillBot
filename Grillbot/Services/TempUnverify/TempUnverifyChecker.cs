using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Config.Dynamic;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyChecker : IDisposable
    {
        private ConfigRepository Repository { get; }
        private Configuration Config { get; }

        public TempUnverifyChecker(ConfigRepository repository, IOptions<Configuration> options)
        {
            Repository = repository;
            Config = options.Value;
        }

        public void Validate(SocketGuildUser user, SocketGuild guild, bool selfunverify, List<string> subjects, List<ulong> currentUnverifiedPersons)
        {
            ValidateSubjects(selfunverify, subjects, guild);
            ValidateServerOwner(guild, user);
            ValidateCurrentlyUnverifiedUsers(user, currentUnverifiedPersons);
            ValidateRoles(guild, user, selfunverify);
            ValidateBotAdmin(selfunverify, user);
        }

        private void ValidateSubjects(bool selfunverify, List<string> subjects, SocketGuild guild)
        {
            if (!selfunverify || subjects?.Count <= 0)
                return;

            var config = Repository.FindConfig(guild.Id, "selfunverify", "").GetData<SelfUnverifyConfig>();

            if (subjects.Count > config.MaxSubjectsCount)
                throw new BotCommandInfoException($"Je možné si ponechat maximálně {config.MaxSubjectsCount} rolí.");

            var invalidSubjects = subjects
                .Select(o => o.ToLower())
                .Where(subject => !config.Subjects.Contains(subject))
                .ToArray();
            
            if (invalidSubjects.Length > 0)
                throw new BotCommandInfoException($"`{string.Join(", ", invalidSubjects)}` nejsou předmětové role.");
        }

        private void ValidateServerOwner(SocketGuild guild, SocketGuildUser user)
        {
            if(guild.OwnerId == user.Id)
                throw new BotCommandInfoException("Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.");
        }

        private void ValidateCurrentlyUnverifiedUsers(SocketGuildUser user, List<ulong> currentlyUnverified)
        {
            if (currentlyUnverified.Any(o => o == user.Id))
                throw new BotCommandInfoException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }

        private void ValidateBotAdmin(bool selfunverify, SocketGuildUser user)
        {
            if (!selfunverify && Config.IsUserBotAdmin(user.Id))
                throw new BotCommandInfoException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je administrátor bota.");
        }

        private void ValidateRoles(SocketGuild guild, SocketGuildUser user, bool selfunverify)
        {
            if (selfunverify) return;

            var botRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            var userMaxRolePosition = user.Roles.Max(o => o.Position);
            if (userMaxRolePosition > botRolePosition)
            {
                var higherRoles = user.Roles.Where(o => o.Position > botRolePosition);
                var higherRoleNames = string.Join(", ", higherRoles.Select(o => o.Name));

                throw new BotCommandInfoException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetFullName()}** má vyšší role. **({higherRoleNames})**");
            }
        }

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}
