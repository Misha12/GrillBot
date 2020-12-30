using Discord;
using Discord.Net;
using Discord.WebSocket;
using Grillbot.Enums;
using System;
using System.Net;
using System.Threading.Tasks;

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

        public static bool IsUser(this IUser user)
        {
            return !user.IsBot && !user.IsWebhook;
        }

        public static async Task SendPrivateMessageAsync(this IUser user, string message = null, Embed embed = null)
        {
            if (!user.IsUser()) return;

            try
            {
                var dmChannel = await user.GetOrCreateDMChannelAsync().ConfigureAwait(false);
                await dmChannel.SendMessageAsync(message?.PreventMassTags(), embed: embed).ConfigureAwait(false);
            }
            catch (HttpException ex)
            {
                if (ex.DiscordCode == (int)DiscordJsonCodes.CannotSendPM)
                    return; // User have disabled PM.

                throw;
            }
        }

        public static Task SendPrivateMessageAsync(this IUser user, string message = null, EmbedBuilder embedBuilder = null)
        {
            return SendPrivateMessageAsync(user, message, embedBuilder.Build());
        }

        public static Task SendPrivateMessageAsync(this IUser user, string message)
        {
            return SendPrivateMessageAsync(user, message, (Embed)null);
        }

        public static string GetDisplayName(this IUser user, bool noDiscriminator = false)
        {
            var shortName = noDiscriminator ? user.Username : user.GetShortName();

            if (user is SocketGuildUser socketGuildUser)
                return string.IsNullOrEmpty(socketGuildUser.Nickname) ? shortName : socketGuildUser.Nickname;
            else
                return shortName;
        }

        public static async Task<byte[]> DownloadAvatarAsync(this IUser user, ushort size = 128)
        {
            var link = user.GetUserAvatarUrl(size: size);

            using var client = new WebClient();
            return await client.DownloadDataTaskAsync(new Uri(link));
        }

        public static async Task<SocketGuildUser> ConvertToGuildUserAsync(this IUser user, SocketGuild guild)
        {
            return user is SocketGuildUser usr ? usr : (await guild.GetUserFromGuildAsync(user.Id));
        }
    }
}
