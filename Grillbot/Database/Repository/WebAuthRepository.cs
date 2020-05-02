using Discord.WebSocket;
using Grillbot.Database.Entity;
using Grillbot.Helpers;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
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
            return FindPermById(guild.Id, user.Id);
        }

        public WebAuthPerm FindPermById(ulong guildID, ulong userID)
        {
            var guildId = guildID.ToString();
            var userId = userID.ToString();

            return Context.WebAdminPerms.FirstOrDefault(o => o.GuildID == guildId && o.ID == userId);
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

            var success = RemoveUser(guild.Id, user.Id);

            if (!success)
                throw new ArgumentException("Tento uživatel neměl nikdy přístup.");
        }

        public bool RemoveUser(ulong guildID, ulong userID)
        {
            var entity = FindPermById(guildID, userID);

            if (entity == null)
                return false;

            Context.Remove(entity);
            Context.SaveChanges();
            return true;
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

        public List<WebAuthPerm> GetAllPerms()
        {
            return Context.WebAdminPerms.ToList();
        }
    }
}
