using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Grillbot.Repository;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Group("hledam")]
    public class TeamModule : BotModuleBase
    {
        private TeamSearchService TeamSearchService { get;}

        public TeamModule(TeamSearchService service)
        {
            TeamSearchService = service;
        }

        [Command("add")]
        public async Task LookingForTeamAsync([Remainder] string message)
        {
           await TeamSearchService.Repository.AddSearch(Context.User, Context.Channel, message);
           await ReplyAsync("Uspesne pridano");
        }

        [Command("info")]
        public async Task TeamSearchInfoAsync()
        {
            var searches = await TeamSearchService.Repository.GetAllSearchesAsync();
            if (!searches.Any())
            {
                await ReplyAsync("Nikdo nic nehleda zatim");
                return;
            }
            
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var x in searches)
            {
                // Ignoring users that are not found in the guild for now
                ulong id = Convert.ToUInt64(x.UserId);
                if(Context.Guild.Users.Any(z => z.Id == id))
                {
                    stringBuilder.AppendLine( $"ID: **{x.Id.ToString()}** - **{Context.Guild.GetUser(Convert.ToUInt64(x.UserId)).Username}** v **{x.Category}** : \"{x.Message}\"");
                }
            }
            
            await Context.User.SendMessageAsync(stringBuilder.ToString());
        }
        
        [Command("remove")]
        public async Task RemoveTeamSearchAsync([Remainder] string stringId)
        {
            int.TryParse(stringId, out int rowId);
            if (rowId == 0)
            {
                await ReplyAsync("Spatne Id");
                return;
            }
            
            var searches = await TeamSearchService.Repository.GetAllSearchesAsync();
            if (searches.All(x => x.Id != rowId))
            {
                await ReplyAsync("Zadna takova zprava neni");
                return;
            }

            var row = searches.First(x => x.Id == rowId);
            ulong.TryParse(row.UserId, out ulong userId);

            if (userId == Context.User.Id)
            {
                await TeamSearchService.Repository.RemoveSearch(rowId);
                await ReplyAsync("Uspesne vymazano");
            }
            else await ReplyAsync("Na to nemas opravneni");
        }

    }
}