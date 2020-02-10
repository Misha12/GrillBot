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
using Grillbot.Database.Entity;
using Grillbot.Services;
using Grillbot.Services.Config.Models;
using Grillbot.Services.Preconditions;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Modules
{
    [Group("hledam")]
    [RequirePermissions]
    [Name("Hledání týmů")]
    public class TeamSearchModule : BotModuleBase
    {
        private TeamSearchService Service { get; }

        private const uint MaxPageSize = 1980;
        private const uint MaxSearchSize = 1900;

        public TeamSearchModule(TeamSearchService service)
        {
            Service = service;
        }

        [Command("add")]
        [Summary("Přidá zprávu o hledání.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public async Task LookingForTeamAsync([Remainder] string messageToAdd)
        {
            if (messageToAdd.Length > MaxSearchSize)
            {
                await ReplyAsync("Zpráva je příliš dlouhá.").ConfigureAwait(false);
                return;
            }

            try
            {
                await Service.AddSearchAsync(Context).ConfigureAwait(false);
                await Context.Message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
            }
            catch
            {
                await Context.Message.AddReactionAsync(new Emoji("❌")).ConfigureAwait(false);
                throw;
            }
        }

        [Command("")]
        [Summary("Vypíše informace o hledání")]
        public async Task TeamSearchInfoAsync()
        {
            var config = GetMethodConfig<TeamSearchConfig>("hledam", "");
            ulong channelId = Context.Channel.Id;

            ulong generalCategoryId = config.GeneralCategoryID;
            var category = (Context.Channel as SocketTextChannel)?.Category?.Id;

            // for now returning if the channel isn't categorized
            if (category == null)
                return;

            var query = Service.Repository.TeamSearch.GetAllSearches();
            bool isMisc = category == generalCategoryId;

            List<TeamSearch> searches;
            if (isMisc)
            {
                var queryData = await query.ToListAsync().ConfigureAwait(false);
                searches = queryData.Where(o => (Context.Guild.GetChannel(Convert.ToUInt64(o.ChannelId)) as SocketTextChannel)?.CategoryId == generalCategoryId)
                    .ToList();
            }
            else
            {
                searches = await query.Where(x => x.ChannelId == channelId.ToString()).ToListAsync().ConfigureAwait(false);
            }

            if (searches.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.").ConfigureAwait(false);
                return;
            }

            var pages = new List<string>();
            var stringBuilder = new StringBuilder();

            foreach (var search in searches)
            {
                // Trying to get the message and checking if it was deleted
                if (!(Context.Guild.GetChannel(Convert.ToUInt64(search.ChannelId)) is ISocketMessageChannel channel))
                    continue;

                var message = await Service.GetMessageAsync(channel.Id, Convert.ToUInt64(search.MessageId)).ConfigureAwait(false);
                if (message == null)
                {
                    // If message was deleted, remove it from Db
                    await Service.Repository.TeamSearch.RemoveSearchAsync(search.Id).ConfigureAwait(false);
                    continue;
                }

                var user = Context.Guild.Users.FirstOrDefault(o => o.Id == message.Author.Id);
                if (user != null)
                {
                    // removes the "!hledam add"
                    string messageContent = message.Content.Remove(0, 12);

                    string userName = user.Nickname ?? user.Username;

                    string mess =
                        $"ID: **{search.Id}** - **{userName}** v" +
                        $" **{channel.Name}** hledá : \"{messageContent}\" [Jump]({message.GetJumpUrl()})";

                    // So the message doesnt overlap across several pages, that limits the message size, but that shouldn't be an issue
                    if (stringBuilder.Length + mess.Length > MaxPageSize)
                    {
                        pages.Add(stringBuilder.ToString());
                        stringBuilder.Clear();
                    }

                    stringBuilder.AppendLine(mess);
                }
            }

            if (stringBuilder.Length != 0) pages.Add(stringBuilder.ToString());

            // if after filters there are no searches don't print empty embed
            if (pages.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.").ConfigureAwait(false);
                return;
            }

            var pagedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Color = Color.Blue,
                Title = isMisc ? "Hledání různě mimo předmětové roomky" : $"Hledání v {Context.Channel.Name}",
                Options = new PaginatedAppearanceOptions() { DisplayInformationIcon = false }
            };

            await PagedReplyAsync(pagedMessage).ConfigureAwait(false);
        }

        [Command("remove")]
        public async Task RemoveTeamSearchAsync([Remainder] string searchId)
        {
            if (!int.TryParse(searchId, out int rowId))
            {
                await ReplyAsync("Neplatný formát ID hledání.").ConfigureAwait(false);
                return;
            }

            var search = await Service.Repository.TeamSearch.FindSearchByID(rowId).ConfigureAwait(false);
            if (search == null)
            {
                await ReplyAsync("Hledaná zpráva neexistuje.").ConfigureAwait(false);
                return;
            }

            // should always work if the row state is correct
            ulong.TryParse(search.UserId, out ulong userId);

            if (userId == Context.User.Id)
            {
                await Service.Repository.TeamSearch.RemoveSearchAsync(rowId).ConfigureAwait(false);
                await Context.Message.AddReactionAsync(new Emoji("✅")).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("Na to nemáš právo.").ConfigureAwait(false);
            }
        }
    }
}