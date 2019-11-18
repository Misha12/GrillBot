using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Statistics;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Logger;
using Grillbot.Services;

namespace Grillbot.Modules
{
    [Name("Stav bota")]
    [RequirePermissions("GrillStatus")]
    public class GetBotStatusModule : BotModuleBase
    {
        private Statistics Statistics { get; }
        private Logger Logger { get; }
        private CalledEventStats CalledEventStats { get; }
        private BotStatusService BotStatusService { get; }

        public GetBotStatusModule(Statistics statistics, Logger logger, CalledEventStats calledEventStats, BotStatusService botStatusService)
        {
            Statistics = statistics;
            Logger = logger;
            CalledEventStats = calledEventStats;
            BotStatusService = botStatusService;
        }

        [Command("grillstatus")]
        [Summary("Vypíše diagnostické informace o botovi.")]
        public async Task StatusAsync() => await StatusAsync("count");

        [Command("grillstatus")]
        [Summary("Vytiskne diagnostické informace o botovi s možností vybrat si řazení statistik metod (orderType).")]
        [Remarks("Možné typy řazení jsou 'time', nebo 'count'.")]
        public async Task StatusAsync(string orderType)
        {
            var data = BotStatusService.GetSimpleStatus();

            var embed = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Stav bota",
            };

            AddInlineEmbedField(embed, "Využití RAM", data.RamUsage);
            AddInlineEmbedField(embed, "Běží od", data.StartTime.ToString("dd. MM. yyyy HH:mm:ss"));
            AddInlineEmbedField(embed, "Počet vláken", data.ThreadStatus);
            AddInlineEmbedField(embed, "Průměrná doba reakce", data.AvgReactTime);
            AddInlineEmbedField(embed, "Instance", data.InstanceType);
            AddInlineEmbedField(embed, "Počet aktivních tokenů", data.ActiveWebTokensCount);

            embed
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync("", embed: embed.Build());
            await PrintCallStatsAsync(orderType == "time");
            await PrintLoggerStatistics();
            await PrintEventStatistics();
        }

        private async Task PrintCallStatsAsync(bool orderByTime)
        {
            var data = BotStatusService.GetCallStats(orderByTime);

            if (data.Count == 0)
                return;

            var embedData = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Title = "Statistiky příkazů"
            };


            AddInlineEmbedField(embedData, "Příkaz", string.Join(Environment.NewLine, data.Select(x => x.Command)));
            AddInlineEmbedField(embedData, "Počet volání", 
                string.Join(Environment.NewLine, data.Select(x => FormatHelper.FormatWithSpaces(x.CallsCount))));
            AddInlineEmbedField(embedData, "Průměrná doba", 
                string.Join(Environment.NewLine, data.Select(o => o.AverageTime + "ms")));

            embedData
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync(embed: embedData.Build());
        }

        private async Task PrintLoggerStatistics()
        {
            var data = BotStatusService.GetLoggerStats();
            if (data.Count == 0) return;

            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Title = "Statistiky logování"
            };

            AddInlineEmbedField(embedBuilder, "Název události", string.Join(Environment.NewLine, data.Select(o => o.Key)));
            AddInlineEmbedField(embedBuilder, "Počet provedení", string.Join(Environment.NewLine, data.Select(o => o.Value)));

            embedBuilder
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync(embed: embedBuilder.Build());
        }

        private async Task PrintEventStatistics()
        {
            var data = CalledEventStats.GetValues();
            
            if (data.Count == 0)
                return;

            var embedBuilder = new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "Statistika zavolaných událostí"
            };

            AddInlineEmbedField(embedBuilder, "Název události", string.Join(Environment.NewLine, data.Select(o => o.Key)));
            AddInlineEmbedField(embedBuilder, "Počet provedení", string.Join(Environment.NewLine, data.Select(o => o.Value)));

            embedBuilder
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}
