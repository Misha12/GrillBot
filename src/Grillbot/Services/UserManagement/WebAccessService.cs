using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Enums.Includes;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class WebAccessService
    {
        private IGrillBotRepository GrillBotRepository { get; }

        public WebAccessService(IGrillBotRepository grillBotRepository)
        {
            GrillBotRepository = grillBotRepository;
        }

        public async Task<string> CreateWebAdminAccessAsync(SocketGuild guild, SocketUser user)
        {
            var plainPassword = StringHelper.CreateRandomString(20);

            var entity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.None);
            entity.WebAdminPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);
            await GrillBotRepository.CommitAsync();

            return plainPassword;
        }

        public async Task RemoveWebAdminAccessAsync(SocketGuild guild, SocketUser user)
        {
            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (string.IsNullOrEmpty(entity?.WebAdminPassword))
                throw new InvalidOperationException($"Uživatel `{user.GetFullName()}` nemá přístup do administrace.");

            entity.WebAdminPassword = null;
            await GrillBotRepository.CommitAsync();
        }

        public async Task<string> CreateApiTokenAsync(SocketGuild guild, SocketUser user)
        {
            var entity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.None);

            entity.ApiToken = Guid.NewGuid().ToString();
            await GrillBotRepository.CommitAsync();

            return entity.ApiToken;
        }

        public async Task RemoveApiTokenAsync(SocketGuild guild, SocketUser user)
        {
            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (string.IsNullOrEmpty(entity?.ApiToken))
                throw new ValidationException("Tento uživatel nemá přístup k API.");

            entity.ApiToken = null;
            await GrillBotRepository.CommitAsync();
        }
    }
}
