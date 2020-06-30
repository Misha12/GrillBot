using Discord;
using Discord.Commands;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Extensions;
using Grillbot.Attributes;
using Grillbot.Helpers;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("userinfo")]
    [ModuleID("UserInfoModule")]
    [Name("Informace o uživateli")]
    public class UserInfoModule : BotModuleBase
    {
        private UserService UserService { get; }

        public UserInfoModule(UserService userService)
        {
            UserService = userService;
        }

        [Command("")]
        [Summary("Informace o konkrétním uživateli.")]
        public async Task UserInfoAsync(IUser identification)
        {
            var user = await Context.Guild.GetUserFromGuildAsync(identification.Id);

            if (user == null)
            {
                await ReplyAsync("Uživatel nebyl na tomto serveru nalezen.");
                return;
            }

            var userDetail = await UserService.GetUserDetailAsync(Context.Guild, user);

            if (userDetail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveru, nebo neprojevil aktivitu.");
                return;
            }

            var mostActiveChannel = userDetail.GetMostActiveChannel();
            var lastActiveChannel = userDetail.GetLastActiveChannel();
            
            var detailFlags = userDetail.GetDetailFlags();
            var clients = userDetail.User.ActiveClients.Select(o => o.ToString());

            var embed = await UserInfoHelper.CreateSimpleEmbedAsync(userDetail, Context);
            embed
                .AddField("Práva", string.Join(", ", userDetail.User.GuildPermissions.GetPermissionsNames()), false);

            if (clients.Any())
                embed.AddField("Aktivní klienti", string.Join(", ", clients), false);

            if (mostActiveChannel != null)
                embed.AddField("Nejaktivnější kanál", $"#{mostActiveChannel.Channel.Name} ({mostActiveChannel.Count.FormatWithSpaces()})", false);

            if (lastActiveChannel != null)
                embed.AddField("Poslední zpráva v", $"#{lastActiveChannel.Channel.Name} ({lastActiveChannel.LastMessageAt.ToLocaleDatetime()})", false);

            embed
                .AddField("Detaily", detailFlags.Count == 0 ? "-" : string.Join(", ", detailFlags), false);

            await ReplyAsync(embed: embed.Build());
        }
    }
}
