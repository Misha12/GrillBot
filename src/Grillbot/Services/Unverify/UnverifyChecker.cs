using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums;
using Grillbot.Database.Enums.Includes;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Unverify
{
    public class UnverifyChecker
    {
        private SearchService SearchService { get; }
        private IGrillBotRepository GrillBotRepository { get; }

        public UnverifyChecker(SearchService searchService, IGrillBotRepository grillBotRepository)
        {
            SearchService = searchService;
            GrillBotRepository = grillBotRepository;
        }

        public async Task ValidateAsync(SocketGuildUser user, SocketGuild guild, bool selfUnverify)
        {
            ValidateServerOwner(guild, user);
            await ValidateUserAsync(user, guild, selfUnverify);
            ValidateRoles(guild, user, selfUnverify);
            await ValidateIfNotUnverifiedAsync(guild, user);
        }

        private void ValidateServerOwner(SocketGuild guild, SocketGuildUser user)
        {
            if (user.IsGuildOwner(guild))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je vlastník serveru.");
        }

        private async Task ValidateUserAsync(SocketGuildUser user, SocketGuild guild, bool selfUnverify)
        {
            if (selfUnverify) return;

            var dbUser = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None, true);

            if (dbUser != null && (dbUser.Flags & (long)UserFlags.BotAdmin) != 0)
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je nejvyšší administrátor bota.");

            if (dbUser != null && !string.IsNullOrEmpty(dbUser.UnverifyImunityGroup))
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** je imunní vůči unverify. Imunitní skupina: **{dbUser.UnverifyImunityGroup}**");
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
            var userID = await SearchService.GetUserIDFromDiscordUserAsync(guild, user);
            var haveUnverify = await GrillBotRepository.UnverifyRepository.HaveUnverifyAsync(userID.Value);

            if (haveUnverify)
                throw new ValidationException($"Nelze provést odebrání přístupu, protože uživatel **{user.GetFullName()}** již má odebraný přístup.");
        }
    }
}
