using Discord;
using Discord.Addons.Interactive;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Models.Config;
using Grillbot.Models.Embed;
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

        protected async Task ReplyChunkedAsync(IEnumerable<EmbedFieldBuilder> fields, BotEmbed embedTemplate, int chunkSize)
        {
            var chunks = fields.SplitInParts(chunkSize);
            await ReplyChunkedAsync(chunks, embedTemplate);
        }

        protected async Task ReplyChunkedAsync(IEnumerable<string> fields, int chunkSize, char separator = '\n')
        {
            var chunks = fields.SplitInParts(chunkSize);
            await ReplyChunkedAsync(chunks, separator);
        }

        protected async Task ReplyChunkedAsync(IEnumerable<IEnumerable<EmbedFieldBuilder>> fieldChunkGroups, BotEmbed embedTemplate)
        {
            foreach(var group in fieldChunkGroups)
            {
                embedTemplate
                    .ClearFields()
                    .WithFields(group);

                await ReplyAsync(embed: embedTemplate.Build());
            }
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
