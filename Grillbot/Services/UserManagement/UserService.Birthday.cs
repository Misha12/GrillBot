using Discord.WebSocket;
using BirthdayDate = Grillbot.Database.Entity.Users.BirthdayDate;
using Grillbot.Database.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.DataAnnotations;
using Grillbot.Models.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public partial class UserService
    {
        public void SetBirthday(SocketGuild guild, SocketUser user, DateTime dateTime, bool acceptAge)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var dbUser = repository.GetOrCreateUser(guild.Id, user.Id, false, true, false, false);

            if (dbUser.Birthday != null)
                throw new ValidationException("Tento uživatel již má uložené datum narození.");

            dbUser.Birthday = new BirthdayDate()
            {
                AcceptAge = acceptAge,
                Date = dateTime
            };

            repository.SaveChanges();
        }

        public void ClearBirthday(SocketGuild guild, SocketUser user)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var dbUser = repository.GetUser(guild.Id, user.Id, false, true, false, false);

            if (dbUser?.Birthday == null)
                throw new ValidationException("Tento uživatel nemá uložené datum narození.");

            dbUser.Birthday = null;
            repository.SaveChanges();
        }

        public async Task<List<DiscordUser>> GetUsersWithTodayBirthdayAsync(SocketGuild guild)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var usersWithBirthday = repository.GetUsersWithBirthday(guild.Id);
            var result = new List<DiscordUser>();
            var today = DateTime.Today;

            foreach(var user in usersWithBirthday.Where(o => o.Birthday.Date.Day == today.Day && o.Birthday.Date.Month == today.Month))
            {
                var mappedUser = await MapUserAsync(user, null);

                if (mappedUser != null)
                    result.Add(mappedUser);
            }

            return result;
        }
    }
}
