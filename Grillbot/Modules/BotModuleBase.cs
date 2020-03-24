using Discord.Addons.Interactive;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Services.Config.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
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

        protected async Task ReplyChunkedAsync(string message, int chunkSize, char separator = '\n')
        {
            var fields = message.Split(separator);
            await ReplyChunkedAsync(fields, chunkSize, separator);
        }

        protected async Task ReplyChunkedAsync(IEnumerable<string> fields, int chunkSize, char separator = '\n')
        {
            var chunks = fields.SplitInParts(chunkSize);
            await ReplyChunkedAsync(chunks, separator);
        }

        protected async Task ReplyChunkedAsync(IEnumerable<IEnumerable<string>> chunkGroups, char separator = '\n')
        {
            foreach(var group in chunkGroups)
            {
                var message = string.Join(separator, group);
                await ReplyAsync(message);
            }
        }
    }
}
