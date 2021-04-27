using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Users;
using System.IO;
using System.Threading.Tasks;
using DBDiscordUser = Grillbot.Database.Entity.Users.DiscordUser;

namespace Grillbot.Helpers
{
    public static class UserHelper
    {
        public static async Task<DiscordUser> MapUserAsync(DiscordSocketClient discord, BotState state, DBDiscordUser dBUser)
        {
            var guild = discord.GetGuild(dBUser.GuildIDSnowflake);

            if (guild == null)
                return null;

            var socketUser = await guild.GetUserFromGuildAsync(dBUser.UserIDSnowflake);
            return await DiscordUser.CreateAsync(guild, socketUser, dBUser, discord, state.AppInfo);
        }

        public static async Task<System.Drawing.Image> DownloadProfilePictureAsync(IUser user, ushort size = 128, bool rounded = false)
        {
            var profileImage = await user.DownloadAvatarAsync(size);
            using var ms = new MemoryStream(profileImage);
            var image = System.Drawing.Image.FromStream(ms);

            try
            {
                if (rounded)
                    return image.RoundImage();

                return image;
            }
            finally
            {
                if (rounded)
                    image.Dispose();
            }
        }
    }
}
