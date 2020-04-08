using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Helpers;
using System;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class WebAuthRepository : RepositoryBase
    {
        public WebAuthRepository(GrillBotContext context) : base(context)
        {
        }

        public WebAuthPerm FindPermById(SocketGuild guild, SocketGuildUser user)
        {
            var guildID = guild.Id.ToString();
            var userID = user.Id.ToString();

            return Context.WebAdminPerms.FirstOrDefault(o => o.GuildID == guildID && o.ID == userID);
        }

        public string AddUser(SocketGuild guild, SocketGuildUser user, string password = null)
        {
            CheckUser(user);

            var entity = new WebAuthPerm()
            {
                GuildIDSnowflake = guild.Id,
                IDSnowflake = user.Id,
            };

            if (string.IsNullOrEmpty(password))
                password = StringHelper.CreateRandomString(20);

            entity.Password = BCrypt.Net.BCrypt.HashPassword(password);

            Context.WebAdminPerms.Add(entity);
            Context.SaveChanges();

            return password;
        }

        public void RemoveUser(SocketGuild guild, SocketGuildUser user)
        {
            CheckUser(user);

            var entity = FindPermById(guild, user);

            if(entity == null)
                throw new ArgumentException("Tento uživatel neměl nikdy přístup.");

            Context.Remove(entity);
            Context.SaveChanges();
        }

        public string ResetPassword(SocketGuild guild, SocketGuildUser user, string password = null)
        {
            CheckUser(user);

            var entity = FindPermById(guild, user);

            if (entity == null)
                throw new ArgumentException("Tento uživatel nemá přístup.");

            if (password == null)
                password = StringHelper.CreateRandomString(20);

            entity.Password = BCrypt.Net.BCrypt.HashPassword(password);

            Context.SaveChanges();
            return password;
        }

        private void CheckUser(SocketGuildUser user)
        {
            if (user == null)
                throw new ArgumentException("Nebyl tagnut žádný uživatel.");
        }
    }
}
