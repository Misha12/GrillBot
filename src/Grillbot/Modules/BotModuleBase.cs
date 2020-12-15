using Discord;
using Discord.Commands;
using Discord.Rest;
using Grillbot.Database;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Permissions.Preconditions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [DisabledChannelCheck]
    [ModuleUnloadCheck]
    [RequirePermissions]
    public abstract class BotModuleBase : ModuleBase<SocketCommandContext>
    {
        private PaginationService PaginationService { get; }
        private IServiceProvider Provider { get; }

        protected BotModuleBase(PaginationService paginationService = null, IServiceProvider provider = null)
        {
            PaginationService = paginationService;
            Provider = provider;
        }

        protected ScopedService<TService> GetService<TService>()
        {
            var scope = Provider.CreateScope();
            var service = scope.ServiceProvider.GetService<TService>();

            return new ScopedService<TService>(service, scope);
        }

        protected async Task<TConfig> GetMethodConfigAsync<TConfig>(string group, string command) where TConfig : class
        {
            if (Provider == null)
                throw new InvalidOperationException("Cannot get method config, missing provider.");

            using var service = GetService<IGrillBotRepository>();

            var config = await service.Service.ConfigRepository.FindConfigAsync(Context.Guild.Id, group, command);
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

        public async Task<RestUserMessage> ReplyFileAsync(byte[] content, string filename)
        {
            using var ms = new MemoryStream(content);

            return await Context.Channel.SendFileAsync(ms, filename);
        }
    }
}
