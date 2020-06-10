using Discord;
using Discord.Commands;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Users;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Extensions;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("userinfo")]
    [Name("Informace o uživateli")]
    public class UserInfoModule : BotModuleBase
    {
        private UserService UserService { get; }

        public UserInfoModule(UserService userService)
        {
            UserService = userService;
        }

        [Command("")]
        [Summary("Informace o uživateli.")]
        public async Task UserInfoAsync()
        {
            var user = await Context.Guild.GetUserFromGuildAsync(Context.User.Id);
            var userDetail = await UserService.GetUserDetailAsync(Context.Guild, user);

            if (userDetail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveu, nebo neprojevil aktivitu.");
                return;
            }

            var embed = CreateSimpleEmbed(userDetail);
            await ReplyAsync(embed: embed.Build());
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

            if(userDetail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveru, nebo neprojevil aktivitu.");
                return;
            }

            var mostActiveChannel = userDetail.GetMostActiveChannel();
            var lastActiveChannel = userDetail.GetLastActiveChannel();
            var selfUnverifies = userDetail.UnverifyHistory.Where(o => o.IsSelfUnverify);
            var detailFlags = userDetail.GetDetailFlags();

            var embed = CreateSimpleEmbed(userDetail);
            embed
                .AddField("Počet unverify (z toho self)", $"{userDetail.UnverifyHistory.Count.FormatWithSpaces()} ({selfUnverifies.Count().FormatWithSpaces()})", true)
                .AddField("Práva", string.Join(", ", userDetail.User.GuildPermissions.GetPermissionsNames()), false)
                .AddField("Aktivní klienti", string.Join(", ", userDetail.User.ActiveClients.Select(o => o.ToString())), false)
                .AddField("Nejaktivnější kanál", $"#{mostActiveChannel.Channel.Name} ({mostActiveChannel.Count.FormatWithSpaces()})", false)
                .AddField("Poslední zpráva v", $"#{lastActiveChannel.Channel.Name} ({lastActiveChannel.LastMessageAt.ToLocaleDatetime()})", false)
                .AddField("Detaily", detailFlags.Count == 0 ? "-" : string.Join(", ", detailFlags), false);

            await ReplyAsync(embed: embed.Build());
        }

        private BotEmbed CreateSimpleEmbed(DiscordUser user)
        {
            var roleWithColor = user.User.Roles.FindHighestRoleWithColor();
            var roleNames = user.User.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Name);

            var embed = new BotEmbed(Context.User, roleWithColor?.Color, "Informace o uživateli", user.User.GetUserAvatarUrl());

            embed
                .AddField("ID", user.User.Id.ToString(), true)
                .AddField("Jméno", user.User.GetFullName(), true)
                .AddField("Stav", user.User.Status.ToString(), true)
                .AddField("Založen", user.User.CreatedAt.LocalDateTime.ToLocaleDatetime(), true)
                .AddField("Připojen", user.User.JoinedAt?.LocalDateTime.ToLocaleDatetime(), true)
                .AddField("Umlčen (Klient/Server)", $"{user.User.IsMuted().TranslateToCz()}/{user.User.IsSelfMuted().TranslateToCz()}", true)
                .AddField("Role", string.Join(", ", roleNames), false);

            if (user.User.PremiumSince != null)
                embed.AddField("Boost od", user.User.PremiumSince.Value.LocalDateTime.ToLocaleDatetime(), true);

            embed
                .AddField("Body", user.Points.FormatWithSpaces(), true)
                .AddField("Reakce (Rozdané/Získané)", user.FormatReactions(), true)
                .AddField("Počet zpráv", user.TotalMessageCount.FormatWithSpaces(), true);

            return embed;
        }
    }
}
