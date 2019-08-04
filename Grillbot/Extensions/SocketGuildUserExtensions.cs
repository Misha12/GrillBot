using Discord.WebSocket;
using System.Text;

namespace Grillbot.Extensions
{
    public static class SocketGuildUserExtensions
    {
        public static string GetUsersFullName(this SocketGuildUser user, bool withoutDiscriminator = false)
        {
            var builder = new StringBuilder()
                .Append(user.Username);

            if(withoutDiscriminator)
            {
                if(!string.IsNullOrEmpty(user.Nickname))
                    builder.Append(" (").Append(user.Nickname).Append(")");
            }
            else
            {
                if (string.IsNullOrEmpty(user.Nickname))
                    builder.Append("#").Append(user.Discriminator);
                else
                    builder.Append(" (").Append(user.Nickname).Append("#").Append(user.Discriminator).Append(")");

            }

            return builder.ToString();
        }

    }
}
