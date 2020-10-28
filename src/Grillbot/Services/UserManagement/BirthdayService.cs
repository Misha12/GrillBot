using Discord.WebSocket;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class BirthdayService : IDisposable
    {
        private UsersRepository UsersRepository { get; }
        private DiscordSocketClient Discord { get; }
        private BotState BotState { get; }

        public BirthdayService(UsersRepository usersRepository, DiscordSocketClient discord, BotState botState)
        {
            UsersRepository = usersRepository;
            Discord = discord;
            BotState = botState;
        }

        public async Task SetBirthdayAsync(SocketGuild guild, SocketUser user, DateTime dateTime, bool acceptAge)
        {
            var dbUser = await UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (dbUser.Birthday != null)
                throw new ValidationException("Tento uživatel již má uložené datum narození.");

            if (acceptAge)
                dbUser.Birthday = dateTime;
            else
                dbUser.Birthday = new DateTime(1, dateTime.Month, dateTime.Day);

            await UsersRepository.SaveChangesAsync();
        }

        public async Task ClearBirthdayAsync(SocketGuild guild, SocketUser user)
        {
            var dbUser = await UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);

            if (dbUser?.Birthday == null)
                throw new ValidationException("Tento uživatel nemá uložené datum narození.");

            dbUser.Birthday = null;
            await UsersRepository.SaveChangesAsync();
        }

        public async Task<List<DiscordUser>> GetUsersWithTodayBirthdayAsync(SocketGuild guild)
        {
            var usersWithBirthday = await UsersRepository.GetUsersWithBirthday(guild.Id).ToListAsync();
            var result = new List<DiscordUser>();

            foreach (var user in usersWithBirthday.Where(o => HaveTodayBirthday(o.Birthday.Value)))
            {
                var mappedUser = await UserHelper.MapUserAsync(Discord, BotState, user);

                if (mappedUser != null)
                    result.Add(mappedUser);
            }

            return result;
        }

        private bool HaveTodayBirthday(DateTime date)
        {
            var today = DateTime.Today;
            return date.Date.Day == today.Day && date.Date.Month == today.Month;
        }

        public async Task<bool> HaveUserBirthday(SocketGuild guild, SocketUser user)
        {
            var dbUser = await UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.None);
            return dbUser?.Birthday != null;
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
        }
    }
}
