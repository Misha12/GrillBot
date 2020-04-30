using Discord;
using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Models.Embed;
using Grillbot.Models.PaginatedEmbed;
using Grillbot.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    public abstract class BotModuleBase : ModuleBase<SocketCommandContext>, IDisposable
    {
        protected Configuration Config { get; }
        protected ConfigRepository ConfigRepository { get; }
        private PaginationService PaginationService { get; }

        protected BotModuleBase(IOptions<Configuration> config = null, ConfigRepository configRepository = null, PaginationService paginationService = null)
        {
            Config = config?.Value;
            ConfigRepository = configRepository;
            PaginationService = paginationService;
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
            foreach (var group in fieldChunkGroups)
            {
                embedTemplate
                    .ClearFields()
                    .WithFields(group);

                await ReplyAsync(embed: embedTemplate.Build());
            }
        }

        protected async Task ReplyChunkedAsync(IEnumerable<IEnumerable<string>> chunkGroups, char separator = '\n')
        {
            foreach (var group in chunkGroups)
            {
                var message = string.Join(separator, group);
                await ReplyAsync(message);
            }
        }

        protected async Task SendPaginatedEmbedAsync(PaginatedEmbed embed)
        {
            if (PaginationService == null)
                throw new InvalidOperationException("Paginated embed requires PaginationService");

            await PaginationService.SendPaginatedMessage(embed, async embed => await ReplyAsync(embed: embed));
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && ConfigRepository != null)
                    ConfigRepository.Dispose();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
