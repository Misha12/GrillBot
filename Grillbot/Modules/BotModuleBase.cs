using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Extensions;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public abstract class BotModuleBase : InteractiveBase
    {
        protected Configuration Config { get; }

        protected BotModuleBase(IOptions<Configuration> config = null)
        {
            Config = config?.Value;
        }

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
                await ReplyAsync(ex.Message.PreventMassTags()).ConfigureAwait(false);
            }
        }

        protected SocketTextChannel GetTextChannel(ulong id) => Context.Guild.GetChannel(id) as SocketTextChannel;

        protected TConfig GetMethodConfig<TConfig>(string group, string command)
        {
            if (Config == null)
                throw new InvalidOperationException("Cannot get method config, missing config instance.");

            using(var repository = new GrillBotRepository(Config))
            {
                var config = repository.Config.FindConfig(Context.Guild.Id, group, command);

                if (config == null)
                    return default;

                return config.GetData<TConfig>();
            }
        }
    }
}
