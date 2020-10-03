using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Models.EmoteStats;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("emoteinfo")]
    [Name("Správa emotů")]
    [ModuleID("EmoteManagerModule")]
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
                .Select(o => new EmbedFieldBuilder().WithName(o.RealID).WithValue(o.GetFormatedInfo()))
                .ToList();

            if (fields.Count == 0)
            {
                await ReplyAsync("Ještě nebyl použit žádný emote.");
                return;
            }

            var embed = GetPaginatedResult(fields, "Kompletní statistika emotů");
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
                .Select(o => new EmbedFieldBuilder().WithName(o.RealID).WithValue(o.GetFormatedInfo()));

            var embed = new BotEmbed(Context.Message.Author)
                .WithFields(fields);

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("")]
        [Summary("Statistika emotů")]
        [Remarks("Parametr 'all' vypíše všechno. Parametr 'asc' vypíše TOP25 vzestupně. Parametr 'desc' vypšíše TOP25 sestupně.")]
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
                {
                    await ReplyAsync("Tento emote neexistuje");
                    return;
                }

                await ReplyAsync("Tento emote ještě nebyl použit");
                return;
            }

            var embed = new BotEmbed(Context.Message.Author)
                .AddField(o => o.WithName(emoteInfo.RealID).WithValue(emoteInfo.GetFormatedInfo()));

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
                case "clear":
                    await ClearOldEmotes();
                    return true;
            }

            return false;
        }

        [Command("unicode")]
        [Summary("Vypíše TOP25 statistika unicode emojis.")]
        private async Task GetEmoteInfoOnlyUnicode()
        {
            var fields = EmoteStats.GetAllUnicodeValues(true, Context.Guild.Id)
                .Select(o => new EmbedFieldBuilder().WithName(o.RealID).WithValue(o.GetFormatedInfo()));

            if (!fields.Any())
            {
                await ReplyAsync("Ještě nebyl použit žádný unicode emote.");
                return;
            }

            var embed = GetPaginatedResult(fields.ToList(), "Kompletní statistika unicode emotů");
            await SendPaginatedEmbedAsync(embed);
        }

        [Command("clear")]
        [Summary("Smazání starých statistik k emotům, které již neexistují.")]
        [Remarks("V případě unicode emoji se smažou ty, které mají 0 použití a nebyly použity déle než 2 týdny.")]
        public async Task ClearOldEmotes()
        {
            var clearedEmotes = await EmoteStats.CleanOldEmotesAsync(Context.Guild).ConfigureAwait(false);

            await ReplyChunkedAsync(clearedEmotes, 10);
            await ReplyAsync("> Čištění dokončeno.");
        }

        [Command("user")]
        [Summary("Statistika používaných emotů daného uživatele.")]
        [Remarks("Je možno zadat řazení pomocí `asc` a `desc`. Pokud se nepodaří správně rozeznat formát, tak bude použito výchozí řazení (Desc).")]
        public async Task GetEmoteInfoOfUser(IUser user, string ascDesc = "desc")
        {
            bool desc = !string.Equals(ascDesc, "asc", StringComparison.InvariantCultureIgnoreCase);

            var emotes = EmoteStats.GetEmoteStatsForUser(Context.Guild, user, desc)
                .Select(o => new GroupedEmoteItem()
                {
                    EmoteID = o.EmoteID,
                    FirstOccuredAt = o.FirstOccuredAt,
                    IsUnicode = o.IsUnicode,
                    LastOccuredAt = o.LastOccuredAt,
                    UseCount = o.UseCount
                })
                .Select(o => new EmbedFieldBuilder().WithName(o.RealID).WithValue(o.GetFormatedInfo(true)))
                .ToList();

            if (!emotes.Any())
            {
                await ReplyAsync("Tento uživatel ještě nepoužil žádný emote.");
                return;
            }

            var embed = GetPaginatedResult(emotes, "Kompletní statistika emotů za uživatele");
            await SendPaginatedEmbedAsync(embed);
        }

        private PaginatedEmbed GetPaginatedResult(List<EmbedFieldBuilder> fields, string title)
        {
            var pages = new List<PaginatedEmbedPage>();

            const int maxFieldsCount = EmbedBuilder.MaxFieldCount - 1;
            var pagesCount = Math.Ceiling((float)fields.Count / maxFieldsCount);
            for (int i = 0; i < pagesCount; i++)
            {
                var page = new PaginatedEmbedPage(null);
                page.AddFields(fields.Skip(i * maxFieldsCount).Take(maxFieldsCount));

                pages.Add(page);
            }

            return new PaginatedEmbed()
            {
                Title = title,
                Pages = pages,
                ResponseFor = Context.User
            };
        }
    }
}