using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Database.Repository;
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
        private TeamSearchService TeamSearchService { get; }

        public TeamSearchModule(ConfigRepository configRepository, TeamSearchService teamSearchService, PaginationService paginationService)
            : base(configRepository, paginationService)
        {
            TeamSearchService = teamSearchService;
        }

        [Command("add")]
        [Summary("Přidá zprávu o hledání.")]
#pragma warning disable IDE0060 // Remove unused parameter
        public async Task LookingForTeamAsync([Remainder] string messageToAdd)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            try
            {
                TeamSearchService.CreateSearch(Context.Guild, Context.User, Context.Channel, Context.Message);
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

        [Command("")]
        [Summary("Vypíše seznam hledání.")]
        public async Task TeamSearchInfoAsync()
        {
            var searches = await TeamSearchService.GetItemsAsync(Context.Channel.Id.ToString());
            await PrintSearchesAsync(searches);
        }

        [Command("full")]
        [Summary("Kompletní seznam hledání napříč kanály")]
        public async Task TeamSearchInfoFullAsync()
        {
            var searches = await TeamSearchService.GetItemsAsync(null);
            await PrintSearchesAsync(searches);
        }

        private async Task PrintSearchesAsync(List<TeamSearchItem> searches)
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
                    .WithName($"**{search.ID}**  - **{search.ShortUsername}** v **{search.ChannelName}**")
                    .WithValue($"\"{search.Message}\" [Jump]({search.MessageLink})");

                currentPage.Add(builder);

                if (currentPage.Count == EmbedBuilder.MaxFieldCount)
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
                Title = $"Hledání v {Context.Channel.Name}"
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
                    await TeamSearchService.RemoveSearchAsync(searchId, user);
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
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public async Task CleanChannelAsync(string channel)
        {
            var state = Context.Channel.EnterTypingState();

            try
            {
                var mentionedChannel = Context.Message.MentionedChannels.FirstOrDefault();

                if (mentionedChannel == null)
                {
                    await ReplyAsync("Nebyl tagnut žádný kanál.");
                    return;
                }

                await TeamSearchService.BatchCleanChannelAsync(mentionedChannel.Id, async message => await ReplyAsync(message));
                await ReplyAsync($"Čištění kanálu `{mentionedChannel.Name}` dokončeno");
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
                await TeamSearchService.BatchCleanAsync(searchIds, async message => await ReplyAsync(message));
                await ReplyAsync("Úklid hledání dokončeno.");
            }
            finally
            {
                state.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                TeamSearchService.Dispose();

            base.Dispose(disposing);
        }
    }
}