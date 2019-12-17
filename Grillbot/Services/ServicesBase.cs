using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public abstract class ServicesBase
    {
        protected async Task<SocketGuildUser> GetUserFromGuildAsync(SocketGuild guild, string userId)
        {
            var idOfUser = Convert.ToUInt64(userId);
            var user = guild.GetUser(idOfUser);

            if (user == null)
            {
                await guild.DownloadUsersAsync();
                user = guild.GetUser(idOfUser);
            }

            return user;
        }

        protected string FixSpaces(string text, int length)
        {
            if (text.Length < length)
                return text.PadRight(length);

            return text;
        }

        protected string GetUsersShortName(SocketUser user)
        {
            return user == null ? "Unknown user" : $"{user.Username}#{user.Discriminator}";
        }
    }
}
