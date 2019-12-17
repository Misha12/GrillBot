using Discord;

namespace Grillbot.Extensions.Discord
{
    public static class UserExtensions
    {
        public static string GetUserAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
        {
            var avatar = user.GetAvatarUrl(format, size);

            return string.IsNullOrEmpty(avatar) ? user.GetDefaultAvatarUrl() : avatar;
        }
    }
}
