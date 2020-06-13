using Discord.WebSocket;
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
        public async Task IncrementWebAdminLoginCount(long userID)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UserStatisticsRepository>();

            await repository.IncrementWebAdminLoginCount(userID);
        }

        public bool AuthenticateWebAccess(SocketGuild guild, SocketGuildUser user, string password, out long userID)
        {
            userID = -1;

            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();
                var userEntity = repository.GetUser(guild.Id, user.Id, false, false, false, false);

                if (string.IsNullOrEmpty(userEntity?.WebAdminPassword))
                    return false;

                userID = userEntity.ID;
                return BCrypt.Net.BCrypt.Verify(password, userEntity.WebAdminPassword);
            }
        }

        public string AddUserToWebAdmin(SocketGuild guild, SocketGuildUser user, string password = null)
        {
            if (!user.IsUser())
                throw new InvalidOperationException("Do administrace lze přidat pouze uživatele.");

            lock (locker)
            {
                using var scope = Services.CreateScope();
                using var repository = scope.ServiceProvider.GetService<UsersRepository>();

                var userEntity = repository.GetOrCreateUser(guild.Id, user.Id, false, false, false, false);
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
                var userEntity = repository.GetUser(guild.Id, user.Id, false, false, false, false);

                if (string.IsNullOrEmpty(userEntity?.WebAdminPassword))
                    throw new ArgumentException("Tento uživatel neměl přístup.");

                userEntity.WebAdminPassword = null;
                repository.SaveChanges();
            }
        }
    }
}
