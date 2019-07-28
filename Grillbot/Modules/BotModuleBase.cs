using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;

namespace Grillbot.Modules
{
    public abstract class BotModuleBase : ModuleBase<SocketCommandContext>
    {
        protected void AddInlineEmbedField(EmbedBuilder embed, string name, object value) =>
            embed.AddField(o => { o.Name = name; o.Value = value; o.IsInline = true; });

        protected string GetUsersFullName(SocketGuildUser user)
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
