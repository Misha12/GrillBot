using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("me")]
    [RequirePermissions]
    [Name("Informace o mě")]
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
            var userDetail = await UserService.GetUserDetailAsync(Context.Guild, user);

            if(userDetail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveu, nebo neprojevil aktivitu.");
                return;
            }

            var embed = await UserInfoHelper.CreateSimpleEmbedAsync(userDetail, Context);
            await ReplyAsync(embed: embed.Build());
        }
    }
}
