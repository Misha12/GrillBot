﻿using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public partial class UserService
    {
        public string GenerateApiToken(SocketGuild guild, SocketUser user)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var dbUser = repository.GetOrCreateUser(guild.Id, user.Id, false, false, false, false, false);

            dbUser.ApiToken = Guid.NewGuid().ToString();

            repository.SaveChanges();
            return dbUser.ApiToken;
        }

        public void ReleaseApiToken(SocketGuild guild, SocketUser user)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var dbUser = repository.GetUser(guild.Id, user.Id, false, false, false, false, false);

            if (dbUser?.ApiToken == null)
                throw new ValidationException("Tento uživatel nikdy nedostal přístup k API.");

            dbUser.ApiToken = null;
            repository.SaveChanges();
        }

        public async Task<DiscordUser> GetUserWithApiTokenAsync(string apiToken)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var user = repository.FindUserByApiToken(apiToken);

            if (user == null)
                return null;

            return await UserHelper.MapUserAsync(DiscordClient, user, null);
        }

        public async Task IncrementApiCallStatistics(string apiToken)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UserStatisticsRepository>();

            await repository.IncrementApiCallCountAsync(apiToken);
        }
    }
}
