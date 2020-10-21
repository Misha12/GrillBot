using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using Grillbot.Services.Statistics.ApiStats;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [ModuleID("ReportModule")]
    [Name("Reporting")]
    [Group("report")]
    public class ReportModule : BotModuleBase
    {
        private InternalStatistics InternalStatistics { get; }
        private BotStatusService BotStatusService { get; }
        private DiscordSocketClient Discord { get; }
        private ApiStatistics ApiStatistics { get; }

        public ReportModule(InternalStatistics internalStatistics, BotStatusService botStatusService, DiscordSocketClient discord, ApiStatistics apiStatistics,
            PaginationService pagination) : base(paginationService: pagination)
        {
            InternalStatistics = internalStatistics;
            BotStatusService = botStatusService;
            Discord = discord;
            ApiStatistics = apiStatistics;
        }

        [Command("commands")]
        [Summary("Report volaných příkazů od startu.")]
        public async Task ReportCommandsAsync()
        {
            var commands = InternalStatistics.GetCommands();

            if (commands.Count == 0)
            {
                await ReplyAsync("Ještě nikdo nevolal žádný příkaz.");
                return;
            }

            var rows = commands.Select(o => $"> {o.Key}: {o.Value.FormatWithSpaces()}");
            await ReplyChunkedAsync(rows, 10);
        }

        [Command("logger")]
        [Summary("Report událostí v loggeru od startu.")]
        public async Task ReportLoggerAsync()
        {
            var events = BotStatusService.GetLoggerStats();

            if (events.Count == 0)
            {
                await ReplyAsync("Ještě neproběhly v loggeru žádné události.");
                return;
            }

            var rows = events.Select(o => $"> {o.Key}: {o.Value.FormatWithSpaces()}");
            await ReplyChunkedAsync(rows, 10);
        }

        [Command("events")]
        [Summary("Report událostí, které přišly z Discordu od startu.")]
        public async Task ReportEventsAsync()
        {
            var events = InternalStatistics.GetEvents();

            if (events.Count == 0)
            {
                await ReplyAsync("Ještě neproběhly žádné události.");
                return;
            }

            var rows = events.Select(o => $"> {o.Key}: {o.Value.FormatWithSpaces()}");
            await ReplyChunkedAsync(rows, 10);
        }

        [Command("db")]
        [Summary("Report stavu databáze.")]
        public async Task ReportDBAsync()
        {
            var tables = await BotStatusService.GetDbReport();

            var rows = tables.Select(o => $"> {o.Key}: {o.Value.Item1.FormatWithSpaces()} ({o.Value.Item2.FormatAsSize()})");
            await ReplyChunkedAsync(rows, 10);
        }

        [Command("memory")]
        [Summary("Report stavu paměti od spuštění.")]
        public async Task ReportMemoryAsync()
        {
            var ramUsage = Process.GetCurrentProcess().WorkingSet64;
            var gc = GC.GetGCMemoryInfo();

            var embed = new BotEmbed(Context.User, title: "Stav paměti")
                .AddField("Využití paměti", ramUsage.FormatAsSize(), true)
                .AddField("Celkem alokováno", GC.GetTotalAllocatedBytes().FormatAsSize(), true)
                .AddField("Ve spravované paměti", GC.GetTotalMemory(false).FormatAsSize(), true)
                .AddField("Fragmentace", gc.FragmentedBytes.FormatAsSize(), true)
                .AddField("Na haldě", gc.HeapSizeBytes.FormatAsSize(), true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("bot")]
        [Summary("Report stavu bota.")]
        public async Task ReportBotStatusAsync()
        {
            var status = BotStatusService.GetSimpleStatus();

            var embed = new BotEmbed(Context.User, title: "Stav klienta")
                .AddField("Instance", status.InstanceType, true)
                .AddField("Odezva", Discord.Latency + " ms", true)
                .AddField("Stav připojení", Discord.ConnectionState.ToString(), true)
                .AddField("Stav přihlášení", Discord.LoginState.ToString(), true)
                .AddField("Vlákna", status.ThreadStatus, true)
                .AddField("Start", status.StartTime.ToLocaleDatetime(), false)
                .AddField("Uptime", (DateTime.Now - status.StartTime).ToString(), false)
                .AddField("CPU čas", status.ActiveCpuTime.ToString(), false);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("api")]
        [Summary("Report volání na Discord API")]
        public async Task ReportAPIAsync()
        {
            var pages = ApiStatistics.Data.Where(o => o.Count > 0).Select(o =>
            {
                var fields = new[]
                {
                    new EmbedFieldBuilder().WithName("Počet volání").WithValue(o.Count.FormatWithSpaces()),
                    new EmbedFieldBuilder().WithName("Minimální čas").WithValue(o.MinTime.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Nejvyšší čas").WithValue(o.MaxTime.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Průměrný čas").WithValue(o.AvgTime.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Celkový čas").WithValue(o.TotalTime.ToString())
                }.ToList();

                return new PaginatedEmbedPage(o.MethodName, fields);
            });

            var embed = new PaginatedEmbed()
            {
                Pages = pages.ToList(),
                ResponseFor = Context.User,
                Title = "Statistika volání na API"
            };

            await SendPaginatedEmbedAsync(embed);
        }
    }
}
