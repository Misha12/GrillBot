using Discord;
using Discord.Commands;
using Grillbot.Exceptions;
using Grillbot.Models.Embed;
using Grillbot.Models.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("emoteinfo")]
    [RequirePermissions]
    [Name("Správa emotů")]
    public class EmoteManagerModule : BotModuleBase
    {
        private EmoteStats EmoteStats { get; }

        public EmoteManagerModule(EmoteStats emoteStats, PaginationService pagination) : base(paginationService: pagination)
        {
            EmoteStats = emoteStats;
        }

        [Command("all")]
        [Summary("Vypíše kompletní statistiku emotů. Seřazeno vzestupně.")]
        public async Task GetCompleteEmoteInfoListAsync()
        {
            var fields = EmoteStats.GetAllValues(true, Context.Guild.Id, true)
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID))
                .Select(o => new EmbedFieldBuilder().WithName(o.GetRealId()).WithValue(o.GetFormatedInfo()))
                .ToList();

            var pages = new List<PaginatedEmbedPage>();

            const int maxFieldsCount = EmbedBuilder.MaxFieldCount - 1;
            var pagesCount = Math.Ceiling((float)fields.Count / maxFieldsCount);
            for (int i = 0; i < pagesCount; i++)
            {
                var page = new PaginatedEmbedPage(null);
                page.AddFields(fields.Skip(i * maxFieldsCount).Take(maxFieldsCount));

                pages.Add(page);
            }

            var embed = new PaginatedEmbed()
            {
                Title = "Kompletní statistika emotů",
                Pages = pages,
                ResponseFor = Context.User
            };

            await SendPaginatedEmbedAsync(embed);
        }

        [Command("desc")]
        [Summary("TOP 25 statistika emotů. Seřazeno sestupně.")]
        public async Task GetTopUsedEmotes()
        {
            await GetTopEmoteUsage(true).ConfigureAwait(false);
        }

        [Command("asc")]
        [Summary("TOP 25 statistika emotů. Seřazeno vzestupně.")]
        public async Task GetTopUsedEmotesAscending()
        {
            await GetTopEmoteUsage(false).ConfigureAwait(false);
        }

        private async Task GetTopEmoteUsage(bool descOrder)
        {
            var fields = EmoteStats.GetAllValues(descOrder, Context.Guild.Id, true)
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID && !x.Animated))
                .Take(EmbedBuilder.MaxFieldCount)
                .Select(o => new EmbedFieldBuilder().WithName(o.GetRealId()).WithValue(o.GetFormatedInfo()));

            var embed = new BotEmbed(Context.Message.Author)
                .WithFields(fields);

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [DisabledPM]
        [Command("")]
        [Summary("Statistika emotu")]
        [Remarks("Parametr 'all' vypíše všechno. Parametr 'asc' vypíše TOP25 vzestupně.  Parametr 'desc' vypšíše TOP25 sestupně.")]
        public async Task GetEmoteInfoAsync([Remainder] string emote)
        {
            if (await GetEmoteInfoAsyncRouting(emote))
                return;

            var existsInGuild = Context.Guild.Emotes.Any(o => o.ToString() == emote);
            var emoteInfo = EmoteStats.GetValue(Context.Guild, emote);

            if (emoteInfo == null)
            {
                var bytes = Encoding.Unicode.GetBytes(emote);
                emoteInfo = EmoteStats.GetValue(Context.Guild, Convert.ToBase64String(bytes));
            }

            if (emoteInfo == null)
            {
                if (!existsInGuild)
                    throw new BotCommandInfoException("Tento emote neexistuje");

                throw new BotCommandInfoException("Tento emote ještě nebyl použit");
            }

            var embed = new BotEmbed(Context.Message.Author)
                .AddField(o => o.WithName(emoteInfo.GetRealId()).WithValue(emoteInfo.GetFormatedInfo()));

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        private async Task<bool> GetEmoteInfoAsyncRouting(string route)
        {
            switch (route)
            {
                case "all":
                    await GetCompleteEmoteInfoListAsync();
                    return true;
                case "asc":
                    await GetTopUsedEmotesAscending();
                    return true;
                case "desc":
                    await GetTopUsedEmotes();
                    return true;
                case "unicode":
                    await GetEmoteInfoOnlyUnicode();
                    return true;
                case "emoteMergeList":
                    await GetMergeListAsync();
                    return true;
                case "processEmoteMerge":
                    await ProcessEmoteMergeAsync();
                    return true;
                case "cleanOldEmotes":
                    await CleanOldEmotes();
                    return true;
            }

            return false;
        }

        [Command("unicode")]
        [Summary("Vypíše TOP25 statistika unicode emojis.")]
        private async Task GetEmoteInfoOnlyUnicode()
        {
            var fields = EmoteStats.GetAllValues(true, Context.Guild.Id, false)
                .Where(o => o.IsUnicode)
                .Take(EmbedBuilder.MaxFieldCount)
                .Select(o => new EmbedFieldBuilder().WithName(o.GetRealId()).WithValue(o.GetFormatedInfo()));

            var embed = new BotEmbed(Context.Message.Author)
                .WithFields(fields);

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("emoteMergeList")]
        [Summary("Seznam potenciálních emotů, které by měli být sloučeny.")]
        public async Task GetMergeListAsync()
        {
            var list = EmoteStats.GetMergeList(Context.Guild);

            if (list.Count == 0)
                throw new BotCommandInfoException("Aktuálně není nic ke sloučení.");

            var embed = new BotEmbed(Context.Message.Author, title: "Seznam potenciálních sloučení emotů");

            embed.WithFields(list.Select(o => new EmbedFieldBuilder()
            {
                Name = $"Target: \\{o.MergeTo}",
                Value = $"Sources: {Environment.NewLine}{string.Join(Environment.NewLine, o.Emotes.Select(x => $"[\\{x.Key}, {x.Value}]"))}"
            }));

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("processEmoteMerge")]
        [Summary("Provede sloučení stejných emotů ve statistikách.")]
        public async Task ProcessEmoteMergeAsync()
        {
            EmoteStats.MergeEmotes(Context.Guild);
            await ReplyAsync("Sloučení dokončeno").ConfigureAwait(false);
        }

        [Command("cleanOldEmotes")]
        [Summary("Smazání starých statistik k emotům, které již neexistují.")]
        public async Task CleanOldEmotes()
        {
            var clearedEmotes = await EmoteStats.CleanOldEmotesAsync(Context.Guild).ConfigureAwait(false);

            await ReplyChunkedAsync(clearedEmotes, 10);
            await ReplyAsync("Čištění dokončeno.").ConfigureAwait(false);
        }
    }
}