using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public abstract class BotModuleBase : ModuleBase<SocketCommandContext>
    {
        protected void AddInlineEmbedField(EmbedBuilder embed, string name, object value) =>
            embed.AddField(o => o.WithIsInline(true).WithName(name).WithValue(value));

        protected string GetUsersFullName(SocketUser user)
        {
            var builder = new StringBuilder();

            if (user is SocketGuildUser sgUser)
            {
                if (string.IsNullOrEmpty(sgUser.Nickname))
                {
                    builder.Append(user.Username).Append("#").Append(user.Discriminator);

                    if (string.IsNullOrEmpty(sgUser.Nickname))
                        builder.Append("#").Append(user.Discriminator);
                    else
                        builder.Append(" (").Append(user.Username).Append("#").Append(user.Discriminator).Append(")");
                }
                else
                {
                    builder.Append(sgUser.Nickname)
                        .Append(" (")
                        .Append(user.Username)
                        .Append("#")
                        .Append(user.Discriminator)
                        .Append(")");
                }
            }
            else
            {
                builder.Append(user.Username).Append("#").Append(user.Discriminator);
            }

            return builder.ToString();
        }

        protected string GetUsersShortName(SocketUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }

        protected async Task DoAsync(Func<Task> method)
        {
            try
            {
                await method();
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }
    }
}
