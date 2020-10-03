using Discord.WebSocket;
using Grillbot.Database.Enums.Includes;
using Grillbot.Database.Repository;
using Grillbot.Helpers;
using Grillbot.Models.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BirthdayDate = Grillbot.Database.Entity.Users.BirthdayDate;

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

        public void SetBirthday(SocketGuild guild, SocketUser user, DateTime dateTime, bool acceptAge)
        {
            var dbUser = UsersRepository.GetOrCreateUser(guild.Id, user.Id, UsersIncludes.Birthday);

            if (dbUser.Birthday != null)
                throw new ValidationException("Tento uživatel již má uložené datum narození.");

            dbUser.Birthday = new BirthdayDate()
            {
                AcceptAge = acceptAge,
                Date = dateTime
            };

            UsersRepository.SaveChanges();
        }

        public void ClearBirthday(SocketGuild guild, SocketUser user)
        {
            var dbUser = UsersRepository.GetUser(guild.Id, user.Id, UsersIncludes.Birthday);

            if (dbUser?.Birthday == null)
                throw new ValidationException("Tento uživatel nemá uložené datum narození.");

            dbUser.Birthday = null;
            UsersRepository.SaveChanges();
        }

        public async Task<List<DiscordUser>> GetUsersWithTodayBirthdayAsync(SocketGuild guild)
        {
            var usersWithBirthday = UsersRepository.GetUsersWithBirthday(guild.Id);
            var result = new List<DiscordUser>();

            foreach (var user in usersWithBirthday.Where(o => UserBirthday.HaveTodayBirthday(o.Birthday.Date)))
            {
                var mappedUser = await UserHelper.MapUserAsync(Discord, BotState, user);

                if (mappedUser != null)
                    result.Add(mappedUser);
            }

            return result;
        }

        public async Task<bool> HaveUserBirthday(SocketGuild guild, SocketUser user)
        {
            var dbUser = UsersRepository.GetUser(guild.Id, user.Id, UsersIncludes.Birthday);
            return dbUser?.Birthday != null;
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
        }
    }
}
