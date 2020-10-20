using Discord.WebSocket;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public partial class UserService
    {
        public async Task IncrementWebAdminLoginCountAsync(long userID)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UserStatisticsRepository>();

            await repository.IncrementWebAdminLoginCount(userID);
        }

        public async Task<long?> AuthenticateWebAccessAsync(SocketGuild guild, SocketGuildUser user, string password)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();
            var userEntity = await repository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (string.IsNullOrEmpty(userEntity?.WebAdminPassword) || !BCrypt.Net.BCrypt.Verify(password, userEntity.WebAdminPassword))
                return null;

            return userEntity.ID;
        }

        public string AddUserToWebAdmin(SocketGuild guild, SocketGuildUser user, string password = null)
        {
            if (!user.IsUser())
                throw new InvalidOperationException("Do administrace lze přidat pouze uživatele.");

            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();

                var userEntity = repository.GetOrCreateUser(guild.Id, user.Id, UsersIncludes.None);
                var plainPassword = string.IsNullOrEmpty(password) ? StringHelper.CreateRandomString(20) : password;
                userEntity.WebAdminPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

                repository.SaveChanges();
                return plainPassword;
            }
        }

        public void RemoveUserFromWebAdmin(SocketGuild guild, SocketGuildUser user)
        {
            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();
                var userEntity = repository.GetUser(guild.Id, user.Id, UsersIncludes.None);

                if (string.IsNullOrEmpty(userEntity?.WebAdminPassword))
                    throw new ArgumentException("Tento uživatel neměl přístup.");

                userEntity.WebAdminPassword = null;
                repository.SaveChanges();
            }
        }
    }
}
