using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Repository.Entity;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Group("hledam")]
    [Name("Hledání týmů")]
    [RequirePermissions("TeamSearch")]
    public class TeamSearchModule : BotModuleBase
    {
        private TeamSearchService Service { get; }

        public TeamSearchModule(TeamSearchService service)
        {
            Service = service;
        }

        [Command("add")]
        [Summary("Přidá zprávu o hledání.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public async Task LookingForTeamAsync([Remainder] string messageToAdd)
        {
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
        
            ulong generalCategoryId = Service.GetGeneralChannelID();
            var category = (Context.Channel as SocketTextChannel)?.Category?.Id;
            
            // for now returning if the channel isn't categorized
            if (category == null)
                return;

            var query = Service.Repository.GetAllSearches();

            List<TeamSearch> searches;
            if (category == generalCategoryId)
            {
                var queryData = await query.ToListAsync();
                searches = queryData.Where(o => GetTextChannel(o.ChannelId)?.CategoryId == generalCategoryId).ToList();
            }
            else
            {
                searches = query.Where(x => x.ChannelId == channelId.ToString()).ToList();
            }
                
            if (searches.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }
    
            var stringBuilder = new StringBuilder();
            foreach (var search in searches)
            {
                // Trying to get the message and checking if it was deleted
                if (!(Context.Guild.Channels.FirstOrDefault(x => x.Id == Convert.ToUInt64(search.ChannelId)) is ISocketMessageChannel channel))
                    continue;

                var message = await channel.GetMessageAsync(Convert.ToUInt64(search.MessageId));
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
                    
                    stringBuilder.AppendLine(
                        $"ID: **{search.Id}** - **{Context.Guild.GetUser(Convert.ToUInt64(search.UserId)).Mention}** v" +
                        $" **{channel.Name}** hledá : \"{messageContent}\"");
                }
            }

            // if after filters there are no searches don't print empty embed
            var description = stringBuilder.ToString();
            if (string.IsNullOrEmpty(description))
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }

            var botUser = Context.Client.CurrentUser;
            var builder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"Hledání v {Context.Channel.Name}")
                .WithDescription(description)
                .WithCurrentTimestamp()
                .WithFooter(botUser.Username, botUser.GetAvatarUrl());

            await ReplyAsync(embed: builder.Build());
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