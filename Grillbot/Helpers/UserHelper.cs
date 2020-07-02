using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;

namespace Grillbot.Helpers
{
    public static class UserHelper
    {
        public static async Task<DiscordUser> MapUserAsync(DiscordSocketClient discord, DBDiscordUser dBUser, List<UserUnverifyHistoryItem> unverifyHistory)
        {
            var guild = discord.GetGuild(dBUser.GuildIDSnowflake);

            if (guild == null)
                return null;

            var socketUser = await guild.GetUserFromGuildAsync(dBUser.UserIDSnowflake);

            if (socketUser == null)
                return null;

            return new DiscordUser(guild, socketUser, dBUser, unverifyHistory);
        }
    }
}
