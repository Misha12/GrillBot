using Discord;
using Discord.Commands;
using Discord.Rest;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Permissions.Preconditions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [DisabledChannelCheck]
    [ModuleUnloadCheck]
    [RequirePermissions]
    public abstract class BotModuleBase : ModuleBase<SocketCommandContext>, IDisposable
    {
        protected ConfigRepository ConfigRepository { get; }
        private PaginationService PaginationService { get; }

        protected BotModuleBase(ConfigRepository configRepository = null, PaginationService paginationService = null)
        {
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

        protected async Task ReplyChunkedAsync(IEnumerable<string> fields, int chunkSize, char separator = '\n')
        {
            var chunks = fields.SplitInParts(chunkSize);
            await ReplyChunkedAsync(chunks, separator);
        }

        protected async Task ReplyChunkedAsync(IEnumerable<IEnumerable<string>> chunkGroups, char separator = '\n')
        {
            var state = Context.Channel.EnterTypingState();

            try
            {
                foreach (var group in chunkGroups)
                {
                    var message = string.Join(separator, group);
                    await ReplyAsync(message);
                }
            }
            finally
            {
                state.Dispose();
            }
        }

        protected async Task SendPaginatedEmbedAsync(PaginatedEmbed embed)
        {
            if (PaginationService == null)
                throw new InvalidOperationException("Paginated embed requires PaginationService");

            await PaginationService.SendPaginatedMessage(embed, async embed => await ReplyAsync(embed: embed));
        }

        protected async Task ReplyAndDeleteAsync(string message = null, bool isTTS = false, Embed embed = null,
            RequestOptions options = null, RequestOptions deleteOptions = null, int timeout = 10)
        {
            var userMessage = await ReplyAsync(message, isTTS, embed, options);

            await Task.Delay(TimeSpan.FromSeconds(timeout));
            await userMessage.DeleteMessageAsync(deleteOptions);
        }

        public async Task<RestUserMessage> ReplyImageAsync(System.Drawing.Image bitmap, string filename)
        {
            using var ms = new MemoryStream();

            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            return await Context.Channel.SendFileAsync(ms, filename);
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
