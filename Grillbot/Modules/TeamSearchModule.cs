using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.AppSettings;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Preconditions;
using Grillbot.Services.TeamSearch;
using Microsoft.Extensions.Options;

namespace Grillbot.Modules
{
    [Group("hledam")]
    [RequirePermissions]
    [Name("Hledání týmů")]
    public class TeamSearchModule : BotModuleBase
    {
        private TeamSearchRepository Repository { get; }
        private IMessageCache MessageCache { get; }
        private TeamSearchService TeamSearchService { get; }

        private const uint MaxPageSize = 1980;

        public TeamSearchModule(TeamSearchRepository repository, IOptions<Configuration> options,
            ConfigRepository configRepository, IMessageCache cache, TeamSearchService teamSearchService) : base(options, configRepository)
        {
            Repository = repository;
            MessageCache = cache;
            TeamSearchService = teamSearchService;
        }

        [Command("add")]
        [Summary("Přidá zprávu o hledání.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public async Task LookingForTeamAsync([Remainder] string messageToAdd)
        {
            await DoAsync(async () =>
            {
                try
                {
                    TeamSearchService.CreateSearch(Context.Guild, Context.User, Context.Channel, Context.Message);
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
                catch
                {
                    await Context.Message.AddReactionAsync(new Emoji("❌"));
                    throw;
                }
            });
        }

        [Command("")]
        [Summary("Vypíše seznam hledání.")]
        public async Task TeamSearchInfoAsync()
        {
            await DoAsync(async () =>
            {
                var searches = await TeamSearchService.GetItemsAsync(Context.Channel.Id.ToString());

                if (searches.Count == 0)
                    throw new ArgumentException("Zatím nikdo nic nehledá.");

                var pages = new List<string>();
                var pageBuilder = new StringBuilder();

                foreach (var search in searches)
                {
                    string message = string.Format("ID: **{0}** - **{1}** v **{2}** hledá: \"{3}\" [Jump]({4})",
                        search.ID, search.ShortUsername, search.ChannelName, search.Message, search.MessageLink);

                    if (pageBuilder.Length + message.Length > MaxPageSize)
                    {
                        pages.Add(pageBuilder.ToString());
                        pageBuilder.Clear();
                    }

                    pageBuilder.AppendLine(message);
                }

                if (pageBuilder.Length != 0)
                    pages.Add(pageBuilder.ToString());

                var paginated = new PaginatedMessage()
                {
                    Pages = pages,
                    Color = Color.Blue,
                    Title = $"Hledání v {Context.Channel.Name}",
                    Options = new PaginatedAppearanceOptions()
                    {
                        DisplayInformationIcon = false
                    }
                };

                await PagedReplyAsync(paginated);
            });
        }

        [Command("remove")]
        public async Task RemoveTeamSearchAsync([Remainder] string searchId)
        {
            if (!int.TryParse(searchId, out int rowId))
            {
                await ReplyAsync("Neplatný formát ID hledání.").ConfigureAwait(false);
                return;
            }

            var search = await Repository.FindSearchByIDAsync(rowId).ConfigureAwait(false);
            if (search == null)
            {
                await ReplyAsync("Hledaná zpráva neexistuje.").ConfigureAwait(false);
                return;
            }

            // should always work if the row state is correct
            ulong.TryParse(search.UserId, out ulong userId);

            if (userId == Context.User.Id)
            {
                await Repository.RemoveSearchAsync(rowId).ConfigureAwait(false);
                await Context.Message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("Na to nemáš právo.").ConfigureAwait(false);
            }
        }

        [Command("cleanChannel")]
        public async Task CleanChannelAsync(string channel)
        {
            await DoAsync(async () =>
            {
                var mentionedChannelID = Context.Message.MentionedChannels.First().Id.ToString();
                var searches = Repository.GetAllSearches(mentionedChannelID);

                if (searches.Count == 0)
                    throw new ArgumentException($"V kanálu {channel.PreventMassTags()} nikdo nic nehledá");

                foreach (var search in searches)
                {
                    var message = await MessageCache.GetAsync(search.ChannelIDSnowflake, search.MessageIDSnowflake);

                    await Repository.RemoveSearchAsync(search.Id);
                    await ReplyAsync($"Hledání s ID **{search.Id}** od **{message.Author.GetFullName()}** smazáno.").ConfigureAwait(false);
                }

                await ReplyAsync($"Čištění kanálu {channel.PreventMassTags()} dokončeno.").ConfigureAwait(false);
            });
        }

        [Command("massRemove")]
        public async Task MassRemoveAsync(params int[] searchIds)
        {
            foreach (var id in searchIds)
            {
                var search = await Repository.FindSearchByIDAsync(id).ConfigureAwait(false);

                if (search != null)
                {
                    var message = await MessageCache.GetAsync(search.ChannelIDSnowflake, search.MessageIDSnowflake);

                    if (message == null)
                        await ReplyAsync($"Úklid neznámého hledání s ID **{id}**.").ConfigureAwait(false);
                    else
                        await ReplyAsync($"Úklid hledání s ID **{id}** od **{message.Author.GetFullName()}**.").ConfigureAwait(false);

                    await Repository.RemoveSearchAsync(id).ConfigureAwait(false);
                }
            }

            await ReplyAsync($"Úklid hledání s ID **{string.Join(", ", searchIds)}** dokončeno.").ConfigureAwait(false);
        }
    }
}