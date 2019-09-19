using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Repository.Entity;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Microsoft.EntityFrameworkCore;
using Grillbot.Extensions;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Group("hledam")]
    [Name("Hledání týmů")]
    [RequirePermissions("TeamSearch")]
    public class TeamSearchModule : InteractiveBase
    {
        private TeamSearchService Service { get; }
        private readonly uint MaxPageSize = 2048;
        private readonly uint MaxSearchSize = 1900;
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
                await ReplyAsync("Zpráva je příliš dlouhá.");
                return;
            }
            
            try
            {
                await Service.AddSearchAsync(Context);
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            catch
            {
                await Context.Message.AddReactionAsync(new Emoji("❌"));
                throw;
            }
        }

        [Command("")]
        [Summary("Vypíše informace o hledání")]
        public async Task TeamSearchInfoAsync()
        {
            ulong channelId = Context.Channel.Id;

            ulong generalCategoryId = Service.GetGeneralCategoryID();
            var category = (Context.Channel as SocketTextChannel)?.Category?.Id;

            // for now returning if the channel isn't categorized
            if (category == null)
                return;

            var query = Service.Repository.GetAllSearches();
            bool isMisc = category == generalCategoryId;

            bool isMisc;
            
            List<TeamSearch> searches;
            if (isMisc)
            {
                var queryData = await query.ToListAsync();
                searches = queryData.Where(o => (Context.Guild.GetChannel(Convert.ToUInt64(o.ChannelId)) as SocketTextChannel)?.CategoryId == generalCategoryId)
                    .ToList();
            }
            else
            {
                searches = await query.Where(x => x.ChannelId == channelId.ToString()).ToListAsync();
            }

            if (searches.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }

            var pages = new List<string>();
            var stringBuilder = new StringBuilder();
            
            foreach (var search in searches)
            {
                // Trying to get the message and checking if it was deleted
                if (!(Context.Guild.GetChannel(Convert.ToUInt64(search.ChannelId)) is ISocketMessageChannel channel))
                    continue;

                var message = await Service.GetMessageAsync(channel.Id, Convert.ToUInt64(search.MessageId));
                if (message == null)
                {
                    // If message was deleted, remove it from Db
                    await Service.Repository.RemoveSearch(search.Id);
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

            if(stringBuilder.Length != 0) pages.Add(stringBuilder.ToString());
            
            // if after filters there are no searches don't print empty embed
            if (pages.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }
            
            // hardcoded var TODO
            var maxPageSize = 2048;

            var pages = stringBuilder.ToString().SplitInParts(maxPageSize);
            var pagedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Color = Color.Blue,
                Title = isMisc ? "Hledání různě mimo předmětové roomky" : $"Hledání v {Context.Channel.Name}",
                Options = new PaginatedAppearanceOptions() { DisplayInformationIcon = false }
            };

            await PagedReplyAsync(pagedMessage);
        }

        [Command("remove")]
        public async Task RemoveTeamSearchAsync([Remainder] string searchId)
        {
            if (!int.TryParse(searchId, out int rowId))
            {
                await ReplyAsync("Neplatný formát ID hledání.");
                return;
            }

            var search = await Service.Repository.FindSearchByID(rowId);
            if (search == null)
            {
                await ReplyAsync("Hledaná zpráva neexistuje.");
                return;
            }

            // should always work if the row state is correct
            ulong.TryParse(search.UserId, out ulong userId);

            if (userId == Context.User.Id)
            {
                await Service.Repository.RemoveSearch(rowId);
                await Context.Message.AddReactionAsync(new Emoji("✅"));
            }
            else
            {
                await ReplyAsync("Na to nemáš právo.");
            }
        }
    }
}