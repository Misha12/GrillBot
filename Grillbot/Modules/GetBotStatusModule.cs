using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Grillbot.Services.Statistics;
using Grillbot.Services.Preconditions;
using Grillbot.Services;
using System.Collections.Generic;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;

namespace Grillbot.Modules
{
    [Name("Stav bota")]
    [Group("status")]
    [RequirePermissions("GrillStatus")]
    public class GetBotStatusModule : BotModuleBase
    {
        private CalledEventStats CalledEventStats { get; }
        private BotStatusService BotStatusService { get; }

        public GetBotStatusModule(CalledEventStats calledEventStats, BotStatusService botStatusService)
        {
            CalledEventStats = calledEventStats;
            BotStatusService = botStatusService;
        }

        [Command("")]
        [Summary("Vytiskne diagnostické informace o botovi.")]
        public async Task StatusAsync()
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
            AddInlineEmbedField(embed, "Aktivní CPU čas", data.ActiveCpuTime);

            embed
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync("", embed: embed.Build());
            await PrintCallStatsAsync();
            await PrintLoggerStatistics();
            await PrintEventStatistics();
        }

        private async Task PrintCallStatsAsync()
        {
            var data = BotStatusService.GetCallStats();

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

        [Command("db")]
        [Summary("Počty záznamů v databázi")]
        public async Task GetDbStatus()
        {
            await DoAsync(async () =>
            {
                var data = await BotStatusService.GetDbReport().ConfigureAwait(false);
                var message = $"```{string.Join(Environment.NewLine, data.Select(x => $"{x.Key} - {x.Value}"))}```";

                await ReplyAsync(message).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("commandLog")]
        [Summary("Log posledních N příkazů.")]
        public async Task GetCommandLog()
        {
            await DoAsync(async () =>
            {
                var data = await BotStatusService.GetCommandLogsAsync();
                var fields = new List<EmbedFieldBuilder>();

                foreach (var o in data)
                {
                    var name = $"{o.ID}: {(!string.IsNullOrEmpty(o.Group) ? $"{o.Group}/{o.Command}" : o.Command)}";

                    var infoFields = new List<string>()
                    {
                        $"Uživatel: **{o.Username}**",
                        $"Kde: **{o.GuildName}/{o.ChannelName}**",
                        $"Kdy: **{o.CalledAt.ToLocaleDatetime()}**"
                    };

                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = name,
                        Value = string.Join(Environment.NewLine, infoFields)
                    });
                }

                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .WithFields(fields)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetUserAvatarUrl())
                    .WithTitle("Log operací")
                    .Build();

                await ReplyAsync(embed: embed);
            });
        }

        [Command("commandLog")]
        [Summary("Log konkrétního záznamu.")]
        public async Task GetCommandLogDetail(string id)
        {
            await DoAsync(async () =>
            {
                var item = await BotStatusService.GetCommandDetailAsync(id);

                if (item == null)
                    throw new ArgumentException($"Záznam s ID **{id}** nebyl nalezen.");

                var name = $"{item.ID}: {(!string.IsNullOrEmpty(item.Group) ? $"{item.Group}/{item.Command}" : item.Command)}";

                var infoFields = new List<string>()
                {
                    $"Uživatel: **{item.Username}**",
                    $"Kde: **{item.GuildName}/{item.ChannelName}**",
                    $"Kdy: **{item.CalledAt.ToLocaleDatetime()}**",
                    $"Příkaz: **{item.FullCommand}**"
                };

                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithCurrentTimestamp()
                    .AddField(o => o.WithName(name).WithValue(string.Join(Environment.NewLine, infoFields)))
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetUserAvatarUrl())
                    .WithTitle($"Log operace s ID {id}")
                    .Build();

                await ReplyAsync(embed: embed);
            });
        }
    }
}
