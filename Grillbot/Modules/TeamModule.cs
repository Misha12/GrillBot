using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Repository;
using Grillbot.Repository.Entity;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Group("hledam")]
    public class TeamModule : BotModuleBase
    {
        private TeamSearchService TeamSearchService { get; }

        public TeamModule(TeamSearchService service)
        {
            TeamSearchService = service;
        }

        [Command("add")]
        public async Task LookingForTeamAsync([Remainder] string message)
        {
            await TeamSearchService.Repository.AddSearch(Context.User, Context.Channel, Context.Message.Id);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
//            await ReplyAsync("Úspěšně přidáno.");
        }

        [Command("info")]
        public async Task TeamSearchInfoAsync()
        {
            ulong channelId = Context.Channel.Id;
        
            // hardcoded id of General category rn (Should be moved to config soon)
            ulong generalCategoryId = 591352457703981067;
            
            var category = ((SocketTextChannel) Context.Channel).Category?.Id;
            
            // for now returning if the channel isn't categorized
            if (category == null) return;
            
            List<TeamSearch> searches;
            if (category == generalCategoryId)
            {
                searches = (await TeamSearchService.Repository.GetAllSearchesAsync()).Where(x => ((SocketTextChannel) Context.Guild.GetChannel(Convert.ToUInt64(x.ChannelId))).CategoryId == generalCategoryId).ToList();
            }
            else
            {
                searches =(await TeamSearchService.Repository.GetAllSearchesAsync())
                    .Where(x => x.ChannelId == channelId.ToString()).ToList();
            }
                
            if (searches.Count == 0)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }
    
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var search in searches)
            {
                // Trying to get the message and checking if it was deleted
                var channel = Context.Guild.Channels.FirstOrDefault(x => x.Id == Convert.ToUInt64(search.ChannelId)) as ISocketMessageChannel;
                if (channel == null) continue;
                
                var message = await channel.GetMessageAsync(Convert.ToUInt64(search.MessageId));
                if (message == null)
                { 
                    // If message was deleted, remove it from Db
                    await TeamSearchService.Repository.RemoveSearch(search.Id);
                    continue;
                }

                if (Context.Guild.Users.Any(z => z.Id == message.Author.Id))
                {
                    // removes the "!hledam add"
                    string messageContent = message.Content.Remove(0, 12);
                    
                    stringBuilder.AppendLine(
                        $"ID: **{search.Id.ToString()}** - **{Context.Guild.GetUser(Convert.ToUInt64(search.UserId)).Mention}** v **{channel.Name}** hledá : \"{messageContent}\"");
                }
            }
            // if after filters there are no searches don't print empty embed
            var description = stringBuilder.ToString();
            if (description == string.Empty)
            {
                await ReplyAsync("Zatím nikdo nic nehledá.");
                return;
            }
            
            var builder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithTitle($"Hledání v {Context.Channel.Name}")
                .WithDescription(description)
                .WithCurrentTimestamp()
                .WithFooter("Grillbot", "https://cdn.discordapp.com/avatars/609618877563011079/934633300c0594c385b49b79ee3aced1.png?size=128");

            await ReplyAsync(embed: builder.Build());
        }

        [Command("remove")]
        public async Task RemoveTeamSearchAsync([Remainder] string stringId)
        {
            if (!int.TryParse(stringId, out int rowId))
            {
                await ReplyAsync("Neplatné ID.");
                return;
            }

            var searches = await TeamSearchService.Repository.GetAllSearchesAsync();
            if (!searches.Any(x => x.Id == rowId))
            {
                await ReplyAsync("Hledaná zpráva neexistuje.");
                return;
            }

            // shouldn't fail as I already checked if such row exists
            var row = searches.First(x => x.Id == rowId);
            // should always work if the row state is correct
            ulong.TryParse(row.UserId, out ulong userId);

            if (userId == Context.User.Id)
            {
                await TeamSearchService.Repository.RemoveSearch(rowId);
                await Context.Message.AddReactionAsync(new Emoji("✅"));
//                await ReplyAsync("Úspěšně vymazáno.");
            }
            else await ReplyAsync("Na to nemáš právo.");
        }
    }
}