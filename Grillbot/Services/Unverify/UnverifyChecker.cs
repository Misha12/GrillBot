using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyChecker : IDisposable
    {
        private Configuration Configuration { get; }
        private UsersRepository UsersRepository { get; }
        private UnverifyRepository UnverifyRepository { get; }

        public UnverifyChecker(IOptions<Configuration> options, UsersRepository repository, UnverifyRepository unverifyRepository)
        {
            Configuration = options.Value;
            UsersRepository = repository;
            UnverifyRepository = unverifyRepository;
        }

        public async Task ValidateAsync(SocketGuildUser user, SocketGuild guild, bool selfUnverify)
        {
            ValidateServerOwner(guild, user);
            ValidateBotAdmin(user, selfUnverify);
            ValidateRoles(guild, user, selfUnverify);
            await ValidateIfNotUnverifiedAsync(guild, user);
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

        private async Task ValidateIfNotUnverifiedAsync(SocketGuild guild, SocketGuildUser user)
        {
            var userID = await UsersRepository.FindUserIDFromDiscordIDAsync(guild.Id, user.Id);
            var haveUnverify = await UnverifyRepository.HaveUnverifyAsync(userID.Value);

            if (haveUnverify)
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
            UnverifyRepository.Dispose();
        }
    }
}
