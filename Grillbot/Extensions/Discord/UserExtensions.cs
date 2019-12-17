using Discord;
using Discord.WebSocket;

namespace Grillbot.Extensions.Discord
{
    public static class UserExtensions
    {
        public static string GetUserAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            var avatar = user.GetAvatarUrl(format, size);

            return string.IsNullOrEmpty(avatar) ? user.GetDefaultAvatarUrl() : avatar;
        }

        public static string GetShortName(this IUser user)
        {
            return user == null ? "Unknown user" : $"{user.Username}#{user.Discriminator}";
        }

        public static string GetFullName(this IUser user)
        {
            if (user is SocketGuildUser sgUser)
            {
                if (string.IsNullOrEmpty(sgUser.Nickname))
                    return user.GetShortName();
                else
                    return $"{sgUser.Nickname} ({user.GetShortName()})";
            }
            else
            {
                return user.GetShortName();
            }
        }
    }
}
