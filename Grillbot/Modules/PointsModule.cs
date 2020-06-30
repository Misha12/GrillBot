using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Práce s body")]
    [RequirePermissions]
    [Group("points")]
    [Alias("body")]
    [ModuleID("PointsModule")]
    public class PointsModule : BotModuleBase
    {
        private PointsService PointsService { get; }

        public PointsModule(PointsService pointsService)
        {
            PointsService = pointsService;
        }

        [Command("")]
        [Summary("Aktuální stav bodů uživatele.")]
        [Alias("where", "gde", "kde", "gdzie")]
        public async Task MyPointsAsync(IUser user = null)
        {
            var userEntity = user ?? Context.User;
            var (points, position) = PointsService.GetPoints(Context.Guild, userEntity);

            var message = $"Uživatel `{userEntity.GetFullName()}` má {FormatPointsValue(points)} a drží se na {position.FormatWithSpaces()} pozici.";
            await ReplyAsync(message);
        }

        [Command("give")]
        [Summary("Přidání/Odebrání bodů.")]
        public async Task GivePointsAsync(int amount, params IUser[] users)
        {
            foreach (var user in users)
            {
                PointsService.GivePoints(Context.User, user, Context.Guild, amount);
            }

            await ReplyAsync($"Body byly úspěšně {(amount > 0 ? "přidány" : "odebrány")}");
        }

        [Command("transfer")]
        [Summary("Převod bodů na jiný účet")]
        public async Task TransferPointsAsync(IUser from, IUser to, long amount = -1)
        {
            var transferedPoints = PointsService.TransferPoints(Context.Guild, from, to, amount);

            await ReplyAsync($"Body byly převedeny.\n{FormatTransferedValue(transferedPoints)}");
        }

        [Command("leaderboard")]
        [Summary("Leaderboard bodů")]
        [Alias("board", "list", "tabule", "seznam", "top10")]
        public async Task PointsLeaderboardAsync(int page = 1)
        {
            var leaderboard = PointsService.GetPointsLeaderboard(Context.Guild, false, page);

            var embed = await CreateLeaderboardResultAsync(leaderboard);
            await ReplyAsync(embed: embed.Build());
        }

        [Command("noleaderboard")]
        [Summary("Leaderboard bodů obráceně")]
        [Alias("boardASC", "listASC", "obracenaTabule", "obracenySeznam", "top10^-1", "bajkar")]
        public async Task PointsAscLeaderboardAsync(int page = 1)
        {
            var leaderboard = PointsService.GetPointsLeaderboard(Context.Guild, true, page);
            var embed = await CreateLeaderboardResultAsync(leaderboard);

            await ReplyAsync(embed: embed.Build());
        }

        private async Task<BotEmbed> CreateLeaderboardResultAsync(List<Tuple<ulong, long, int>> leaderboard)
        {
            var embed = new BotEmbed(Context.User, null, "Body leaderboard");

            if (leaderboard.Count == 0)
            {
                embed.WithDescription("Na této stránce není moc k vidění.");
                return embed;
            }

            var builder = new StringBuilder();

            foreach (var item in leaderboard)
            {
                var user = await Context.Guild.GetUserFromGuildAsync(item.Item1);

                var position = item.Item3.FormatWithSpaces();
                var username = user == null ? "Neexistující uživatel" : user.GetDisplayName();

                builder.AppendLine($"{position}: {username}: {FormatPointsValue(item.Item2)}");
            }

            embed.WithDescription(builder.ToString());
            return embed;
        }

        private string FormatTransferedValue(long transferedPoints)
        {
            if (transferedPoints == 1)
                return "Převeden **1** bod";

            if (transferedPoints == 0 || transferedPoints > 4)
                return $"Převedeno {FormatPointsValue(transferedPoints)}";

            return $"Převedeny {FormatPointsValue(transferedPoints)}";
        }

        private string FormatPointsValue(long points)
        {
            if (points == 0 || points > 4)
                return $"**{points.FormatWithSpaces()}** bodů";

            return points == 1 ? "**1** bod" : $"**{points.FormatWithSpaces()}** body";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                PointsService.Dispose();

            base.Dispose(disposing);
        }
    }
}
