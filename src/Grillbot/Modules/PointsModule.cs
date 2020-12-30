using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.UserManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Práce s body")]
    [Group("points")]
    [Alias("body", "punkty")]
    [ModuleID(nameof(PointsModule))]
    [Remarks("Body se počítají podobným způsobem jako MEE6. Jednou za minutu 15-25 bodů za každou zprávu.\nPočítají se i reakce. " +
        "Princip u reakcí je stejný jako u zprávy. Jen omezení je jednou za půl minuty a rozsah je 0-10 bodů.")]
    public class PointsModule : BotModuleBase
    {
        public PointsModule(IServiceProvider provider) : base(provider: provider)
        {
        }

        [Command("where")]
        [Summary("Aktuální stav bodů uživatele.")]
        [Alias("gde", "kde", "gdzie")]
        public async Task MyPointsAsync(IUser user = null)
        {
            var userEntity = user ?? Context.User;

            using var service = GetService<PointsService>();

            using var image = await service.Service.GetPointsAsync(Context.Guild, userEntity);
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            await Context.Channel.SendFileAsync(ms, $"points_{userEntity.Username}.png");
        }

        [Command("give")]
        [Summary("Přidání/Odebrání bodů.")]
        public async Task GivePointsAsync(int amount, params IUser[] users)
        {
            using var service = GetService<PointsService>();

            var tasks = users
                .Select(o => service.Service.GivePointsAsync(Context.User, o, Context.Guild, amount))
                .ToArray();

            await Task.WhenAll(tasks);
            await ReplyAsync($"Body byly úspěšně {(amount > 0 ? "přidány" : "odebrány")}");
        }

        [Command("transfer")]
        [Summary("Převod bodů na jiný účet")]
        public async Task TransferPointsAsync(IUser from, IUser to, long amount = -1)
        {
            using var service = GetService<PointsService>();
            var transferedPoints = await service.Service.TransferPointsAsync(Context.Guild, from, to, amount);

            await ReplyAsync($"Body byly převedeny.\n{FormatTransferedValue(transferedPoints)}");
        }

        [Command("leaderboard")]
        [Summary("Leaderboard bodů")]
        [Alias("board", "list", "top10")]
        public async Task PointsLeaderboardAsync(int page = 1)
        {
            using var service = GetService<PointsService>();
            var leaderboard = await service.Service.GetPointsLeaderboardAsync(Context.Guild, false, page);

            var embed = await CreateLeaderboardResultAsync(leaderboard.Item1, leaderboard.Item2);
            await ReplyAsync(embed: embed);
        }

        [Command("noleaderboard")]
        [Summary("Leaderboard bodů obráceně")]
        [Alias("boardASC", "listASC", "obracenyList", "top10^-1", "bajkar")]
        public async Task PointsAscLeaderboardAsync(int page = 1)
        {
            using var service = GetService<PointsService>();
            var leaderboard = await service.Service.GetPointsLeaderboardAsync(Context.Guild, true, page);
            var embed = await CreateLeaderboardResultAsync(leaderboard.Item1, leaderboard.Item2);

            await ReplyAsync(embed: embed);
        }

        private async Task<Embed> CreateLeaderboardResultAsync(List<Tuple<ulong, long>> leaderboardData, int skip)
        {
            var leaderboard = new LeaderboardBuilder("Body leaderboard", Context.User, null, null)
            {
                Skip = skip
            };

            foreach (var item in leaderboardData)
            {
                var user = await Context.Guild.GetUserFromGuildAsync(item.Item1);
                var username = user == null ? "Neexistující uživatel" : user.GetDisplayName(true);

                leaderboard.AddItem(username, FormatPointsValue(item.Item2));
            }

            return leaderboard.Build();
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
    }
}
