using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Grillbot.Services.TempUnverify
{
    public class TempUnverifyChecker : IDisposable
    {
        private Configuration Config { get; }
        private TempUnverifyRepository Repository { get; }

        public TempUnverifyChecker(IOptions<Configuration> options, TempUnverifyRepository repository)
        {
            Repository = repository;
            Config = options.Value;
        }

        public void Validate(SocketGuildUser user, SocketGuild guild, bool selfunverify)
        {
            ValidateServerOwner(guild, user);
            ValidateCurrentlyUnverifiedUsers(user);
            ValidateRoles(guild, user, selfunverify);
            ValidateBotAdmin(selfunverify, user);
        }

        private void ValidateServerOwner(SocketGuild guild, SocketGuildUser user)
        {
            if(guild.OwnerId == user.Id)
                throw new ValidationException("Nelze provést odebrání přístupu, protože se mezi uživateli nachází vlastník serveru.");
        }

        private void ValidateCurrentlyUnverifiedUsers(SocketGuildUser user)
        {
            if(Repository.UnverifyExists(user.Id))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }

        private void ValidateBotAdmin(bool selfunverify, SocketGuildUser user)
        {
            if (!selfunverify && Config.IsUserBotAdmin(user.Id))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je administrátor bota.");
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

                throw new ValidationException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetFullName()}** má vyšší role. **({higherRoleNames})**");
            }
        }

        public void Dispose()
        {
            Repository.Dispose();
        }
    }
}
