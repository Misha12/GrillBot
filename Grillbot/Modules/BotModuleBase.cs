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

        [Obsolete("Use UserExtension GetFullName")]
        protected string GetUsersFullName(IUser user)
        {
            var builder = new StringBuilder();

            if (user is SocketGuildUser sgUser)
            {
                if (string.IsNullOrEmpty(sgUser.Nickname))
                {
                    builder
                        .Append(user.Username)
                        .Append("#")
                        .Append(user.Discriminator);
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

        [Obsolete("Use UserExtension GetShortName")]
        protected string GetUsersShortName(SocketUser user)
        {
            return user == null ? "Unknown user" : $"{user.Username}#{user.Discriminator}";
        }

        protected async Task DoAsync(Func<Task> method)
        {
            try
            {
                await method().ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message).ConfigureAwait(false);
            }
        }

        protected SocketTextChannel GetTextChannel(string id) => GetTextChannel(Convert.ToUInt64(id));

        protected SocketTextChannel GetTextChannel(ulong id) => Context.Guild.GetChannel(id) as SocketTextChannel;

        protected async Task<SocketGuildUser> GetUserFromGuildAsync(SocketGuild guild, string userId)
        {
            var idOfUser = Convert.ToUInt64(userId);
            var user = guild.GetUser(idOfUser);

            if (user == null)
            {
                await guild.DownloadUsersAsync().ConfigureAwait(false);
                user = guild.GetUser(idOfUser);
            }

            return user;
        }
    }
}
