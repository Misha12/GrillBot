using Discord.WebSocket;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyChecker : IDisposable
    {
        private UsersRepository UsersRepository { get; }
        private UnverifyRepository UnverifyRepository { get; }
        private UserSearchService UserSearchService { get; }

        public UnverifyChecker(UsersRepository repository, UnverifyRepository unverifyRepository, UserSearchService searchService)
        {
            UsersRepository = repository;
            UnverifyRepository = unverifyRepository;
            UserSearchService = searchService;
        }

        public async Task ValidateAsync(SocketGuildUser user, SocketGuild guild, bool selfUnverify)
        {
            ValidateServerOwner(guild, user);
            await ValidateBotAdminAsync(user, guild, selfUnverify);
            ValidateRoles(guild, user, selfUnverify);
            await ValidateIfNotUnverifiedAsync(guild, user);
        }

        private void ValidateServerOwner(SocketGuild guild, SocketGuildUser user)
        {
            if (user.IsGuildOwner(guild))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je vlastník serveru.");
        }

        private async Task ValidateBotAdminAsync(SocketGuildUser user, SocketGuild guild, bool selfUnverify)
        {
            if (selfUnverify) return;

            var dbUser = await UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (dbUser != null && (dbUser.Flags & (long)UserFlags.BotAdmin) != 0)
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je nejvyšší administrátor bota.");
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
            var userID = await UserSearchService.GetUserIDFromDiscordAsync(guild, user);
            var haveUnverify = await UnverifyRepository.HaveUnverifyAsync(userID.Value);

            if (haveUnverify)
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
            UnverifyRepository.Dispose();
            UserSearchService.Dispose();
        }
    }
}
