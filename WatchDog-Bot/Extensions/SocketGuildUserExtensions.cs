using Discord.WebSocket;
using System.Text;

namespace WatchDog_Bot.Extensions
{
    public static class SocketGuildUserExtensions
    {
        public static string GetUsersFullName(this SocketGuildUser user)
        {
            var builder = new StringBuilder()
                .Append(user.Username);

            if (string.IsNullOrEmpty(user.Nickname))
                builder.Append("#").Append(user.Discriminator);
            else
                builder.Append(" (").Append(user.Nickname).Append("#").Append(user.Discriminator).Append(")");

            return builder.ToString();
        }
    }
}
