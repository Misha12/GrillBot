using Discord;
using Discord.Commands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Modules
{
    [Name("Stav bota")]
    public class GetBotStatusModule : BotModuleBase
    {
        private Statistics Statistics { get; }
        private IConfiguration Config { get; }

        public GetBotStatusModule(Statistics statistics, IConfiguration config)
        {
            Statistics = statistics;
            Config = config;
        }

        [Command("grillstatus")]
        [Summary("Vypíše diagnostické informace o botovi.")]
        [RequireRoleOrAdmin(RoleGroupName = "GrillStatus")]
        [DisabledCheck(RoleGroupName = "GrillStatus")]
        public async Task Status()
        {
            await Status("count");
        }

        [Command("grillstatus")]
        [Summary("Vytiskne diagnostické informace o botovi s možností vybrat si řazení statistik metod (orderType).")]
        [Remarks("Možné typy řazení jsou 'time', nebo 'count'.")]
        [RequireRoleOrAdmin(RoleGroupName = "GrillStatus")]
        [DisabledCheck(RoleGroupName = "GrillStatus")]
        public async Task Status(string orderType)
        {
            var processStatus = Process.GetCurrentProcess();

            var embed = new EmbedBuilder()
            {
                Color = Color.Blue,
                Description = "Stav bota",
            };

            AddInlineEmbedField(embed, "Využití RAM", FormatHelper.FormatAsSize(processStatus.WorkingSet64));
            AddInlineEmbedField(embed, "Běží od", processStatus.StartTime.ToString("dd. MM. yyyy HH:mm:ss"));
            AddInlineEmbedField(embed, "Počet vláken", GetThreadStatus(processStatus));
            AddInlineEmbedField(embed, "Průměrná doba reakce", Statistics.GetAvgReactTime());
            AddInlineEmbedField(embed, "Instance", GetInstanceType());

            await ReplyAsync("", embed: embed.Build());
            await PrintCallStats(orderType == "time");
        }

        private async Task PrintCallStats(bool orderByTime)
        {
            var data = Statistics.GetOrderedData(orderByTime);

            var embedData = new EmbedBuilder()
            {
                Color = Color.DarkBlue,
                Description = "Statistiky příkazů"
            };


            AddInlineEmbedField(embedData, "Příkaz", string.Join(Environment.NewLine, data.Select(x => x.Command)));
            AddInlineEmbedField(embedData, "Počet volání", 
                string.Join(Environment.NewLine, data.Select(x => FormatHelper.FormatWithSpaces(x.CallsCount))));
            AddInlineEmbedField(embedData, "Průměrná doba", 
                string.Join(Environment.NewLine, data.Select(o => o.AverageTime + "ms")));

            await ReplyAsync("", embed: embedData.Build());
        }

        private string GetThreadStatus(Process process)
        {
            int sleepCount = 0;
            var sleepCounter = process.Threads.GetEnumerator();
            while (sleepCounter.MoveNext())
                if ((sleepCounter.Current as ProcessThread)?.ThreadState == ThreadState.Wait)
                    sleepCount++;

            return $"{FormatHelper.FormatWithSpaces(process.Threads.Count)} ({FormatHelper.FormatWithSpaces(sleepCount)} spí)";
        }

        public string GetInstanceType()
        {
            var configValue = Config["IsDevelopment"];
            if (string.IsNullOrEmpty(configValue)) return "Release";

            return Convert.ToBoolean(configValue) ? "Development" : "Release";
        }
    }
}
