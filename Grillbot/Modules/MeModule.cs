using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Services.UserManagement;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("me")]
    [Name("Informace o mně")]
    [ModuleID("MeModule")]
    public class MeModule : BotModuleBase
    {
        private UserService UserService { get; }

        public MeModule(UserService userService)
        {
            UserService = userService;
        }

        [Command("")]
        [Summary("<FromModule(Name)>")]
        public async Task InfoAboutMeAsync()
        {
            var user = Context.User is SocketGuildUser usr ? usr : await Context.Guild.GetUserFromGuildAsync(Context.User.Id);
            var detail = await UserService.GetUserInfoAsync(Context.Guild, user);

            if(detail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveu, nebo neprojevil aktivitu.");
                return;
            }

            var embed = await UserInfoHelper.CreateSimpleEmbedAsync(detail, Context);
            await ReplyAsync(embed: embed.Build());
        }
    }
}
