using Discord;
using Discord.Addons.Interactive;
using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
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
        protected ConfigRepository ConfigRepository { get; }

        protected BotModuleBase(IOptions<Configuration> config = null, ConfigRepository configRepository = null)
        {
            Config = config?.Value;
            ConfigRepository = configRepository;
        }

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

        protected TConfig GetMethodConfig<TConfig>(string group, string command) where TConfig : class
        {
            if (ConfigRepository == null)
                throw new InvalidOperationException("Cannot get method config, missing config instance.");

            var config = ConfigRepository.FindConfig(Context.Guild.Id, group, command);
            return config?.GetData<TConfig>() ?? throw new ConfigException();
        }
    }
}
