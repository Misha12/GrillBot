using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Models.Config.AppSettings;
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
        private TeamSearchService TeamSearchService { get; }

        private const uint MaxPageSize = 1980;

        public TeamSearchModule(IOptions<Configuration> options, ConfigRepository configRepository, TeamSearchService teamSearchService)
            : base(options, configRepository)
        {
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
        public async Task RemoveTeamSearchAsync(int searchId)
        {
            await DoAsync(async () =>
            {
                if (Context.User is SocketGuildUser user)
                {
                    TeamSearchService.RemoveSearch(searchId, user);
                    await Context.Message.AddReactionAsync(new Emoji("✅"));
                }
            });
        }

        [Command("cleanChannel")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public async Task CleanChannelAsync(string channel)
        {
            await DoAsync(async () =>
            {
                var mentionedChannel = Context.Message.MentionedChannels.FirstOrDefault();

                if (mentionedChannel == null)
                    throw new ArgumentException("Nebyl tagnut žádný kanál.");

                await TeamSearchService.BatchCleanChannelAsync(mentionedChannel.Id, async message => await ReplyAsync(message));
                await ReplyAsync($"Čištění kanálu `{mentionedChannel.Name}` dokončeno");
            });
        }

        [Command("massRemove")]
        public async Task MassRemoveAsync(params int[] searchIds)
        {
            await TeamSearchService.BatchCleanAsync(searchIds, async message => await ReplyAsync(message));
            await ReplyAsync("Úklid hledání dokončeno.");
        }
    }
}