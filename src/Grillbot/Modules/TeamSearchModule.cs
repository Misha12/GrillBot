using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Helpers;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Models.TeamSearch;
using Grillbot.Services;
using Grillbot.Services.TeamSearch;

namespace Grillbot.Modules
{
    [Group("hledam")]
    [Name("Hledání týmů")]
    [ModuleID("TeamSearchModule")]
    public class TeamSearchModule : BotModuleBase
    {
        public TeamSearchModule(IServiceProvider provider, PaginationService paginationService) : base(paginationService, provider)
        {
        }

        [Command("")]
        [Summary("Přidá zprávu o hledání.")]
        public async Task LookingForTeamAsync([Remainder] string messageToAdd)
        {
            if (await RouteTeamSearchAsync(messageToAdd))
                return;

            try
            {
                using var service = GetService<TeamSearchService>();

                await service.Service.CreateSearchAsync(Context.Guild, Context.User, Context.Channel, Context.Message);
                await Context.Message.AddReactionAsync(ReactHelpers.OKEmoji);
            }
            catch (Exception ex)
            {
                await Context.Message.AddReactionAsync(ReactHelpers.NOKEmoji);

                if (ex is ValidationException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        private async Task<bool> RouteTeamSearchAsync(string route)
        {
            var fields = route.Split(' ').Select(o => o.Trim()).ToArray();

            switch (fields[0].ToLower())
            {
                case "full":
                    await TeamSearchInfoFullAsync();
                    return true;
                case "remove":
                    await RemoveTeamSearchAsync(Convert.ToInt32(fields[1]));
                    return true;
                case "massremove":
                    await MassRemoveAsync(fields.Skip(1).Select(o => Convert.ToInt32(o)).ToArray());
                    return true;
            }

            return false;
        }

        [Command("")]
        [Summary("Vypíše seznam hledání.")]
        public async Task TeamSearchInfoAsync()
        {
            using var service = GetService<TeamSearchService>();
            var searches = await service.Service.GetItemsAsync(Context.Channel.Id.ToString());
            await PrintSearchesAsync(searches);
        }

        [Command("full")]
        [Summary("Kompletní seznam hledání napříč kanály")]
        public async Task TeamSearchInfoFullAsync()
        {
            using var service = GetService<TeamSearchService>();
            var searches = await service.Service.GetItemsAsync(null);
            await PrintSearchesAsync(searches, true);
        }

        private async Task PrintSearchesAsync(List<TeamSearchItem> searches, bool allChannels = false)
        {
            if (searches.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }

            var pages = new List<PaginatedEmbedPage>();
            var currentPage = new List<EmbedFieldBuilder>();

            foreach (var search in searches)
            {
                var builder = new EmbedFieldBuilder()
                    .WithName($"**{search.ID}**  - **{search.ShortUsername}**{(allChannels ? $" v **{search.ChannelName}**" : "")}")
                    .WithValue($"\"{search.Message}\" [Jump]({search.MessageLink})");

                currentPage.Add(builder);

                if (currentPage.Count == 10)
                {
                    pages.Add(new PaginatedEmbedPage(null, new List<EmbedFieldBuilder>(currentPage)));
                    currentPage.Clear();
                }
            }

            if (currentPage.Count != 0)
                pages.Add(new PaginatedEmbedPage(null, new List<EmbedFieldBuilder>(currentPage)));

            var embed = new PaginatedEmbed()
            {
                Pages = pages,
                ResponseFor = Context.User,
                Title = allChannels ? "Hledání" : $"Hledání v {Context.Channel.Name}"
            };

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("remove")]
        public async Task RemoveTeamSearchAsync(int searchId)
        {
            try
            {
                if (Context.User is SocketGuildUser user)
                {
                    using var service = GetService<TeamSearchService>();
                    await service.Service.RemoveSearchAsync(searchId, user);
                    await Context.Message.AddReactionAsync(ReactHelpers.OKEmoji);
                }
            }
            catch (Exception ex)
            {
                await Context.Message.AddReactionAsync(ReactHelpers.NOKEmoji);

                if (ex is ValidationException || ex is UnauthorizedAccessException)
                {
                    await ReplyAsync(ex.Message);
                    return;
                }

                throw;
            }
        }

        [Command("cleanChannel")]
        public async Task CleanChannelAsync(IChannel channel)
        {
            var state = Context.Channel.EnterTypingState();

            try
            {
                using var service = GetService<TeamSearchService>();

                var messages = await service.Service.BatchCleanChannelAsync(channel.Id);

                await ReplyChunkedAsync(messages, 5);
                await ReplyAsync($"Čištění kanálu `{channel.Name}` dokončeno");
            }
            finally
            {
                state.Dispose();
            }
        }

        [Command("massRemove")]
        public async Task MassRemoveAsync(params int[] searchIds)
        {
            var state = Context.Channel.EnterTypingState();

            try
            {
                using var service = GetService<TeamSearchService>();

                var messages = await service.Service.BatchCleanAsync(searchIds);

                await ReplyChunkedAsync(messages, 5);
                await ReplyAsync("Úklid hledání dokončeno.");
            }
            finally
            {
                state.Dispose();
            }
        }
    }
}