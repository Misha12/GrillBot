using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public abstract class BotModuleBase : InteractiveBase
    {
        protected void AddInlineEmbedField(EmbedBuilder embed, string name, object value) =>
            embed.AddField(o => o.WithIsInline(true).WithName(name).WithValue(value));

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

        protected SocketTextChannel GetTextChannel(ulong id) => Context.Guild.GetChannel(id) as SocketTextChannel;
    }
}
