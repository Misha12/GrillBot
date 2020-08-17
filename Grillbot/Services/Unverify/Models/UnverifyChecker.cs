using Discord.WebSocket;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Grillbot.Services.Unverify.Models
{
    public class UnverifyChecker : IDisposable
    {
        private Configuration Configuration { get; }
        private UsersRepository UsersRepository { get; }

        public UnverifyChecker(IOptions<Configuration> options, UsersRepository repository)
        {
            Configuration = options.Value;
            UsersRepository = repository;
        }

        public void Validate(SocketGuildUser user, SocketGuild guild, bool selfUnverify)
        {
            ValidateServerOwner(guild, user);
            ValidateBotAdmin(user, selfUnverify);
            ValidateRoles(guild, user, selfUnverify);
            ValidateIfNotUnverified(guild, user);
        }

        private void ValidateServerOwner(SocketGuild guild, SocketGuildUser user)
        {
            if (user.IsGuildOwner(guild))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je vlastník serveru.");
        }

        private void ValidateBotAdmin(SocketGuildUser user, bool selfUnverify)
        {
            if (!selfUnverify && Configuration.IsUserBotAdmin(user.Id))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }

        private void ValidateRoles(SocketGuild guild, SocketGuildUser user, bool selfUnverify)
        {
            if (selfUnverify) return;

            var botRolePosition = guild.CurrentUser.Roles.Max(o => o.Position);
            var userMaxRolePosition = user.Roles.Max(o => o.Position);

            if (userMaxRolePosition > botRolePosition)
            {
                var higherRoles = user.Roles.Where(o => o.Position > botRolePosition);
                var higherRoleNames = string.Join(", ", higherRoles.Select(o => o.Name));

                throw new ValidationException($"Nelze provést odebírání přístupu, protože uživatel **{user.GetFullName()}** má vyšší role. **({higherRoleNames})**");
            }
        }

        private void ValidateIfNotUnverified(SocketGuild guild, SocketGuildUser user)
        {
            var userEntity = UsersRepository.GetUser(guild.Id, user.Id, UsersIncludes.Unverify);

            if (userEntity?.Unverify != null)
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
        }
    }
}
