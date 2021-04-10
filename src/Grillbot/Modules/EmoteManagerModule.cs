using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Enums;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Models.EmoteStats;
using Grillbot.Services;
using Grillbot.Services.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("emoteinfo")]
    [Name("Správa emotů")]
    [ModuleID(nameof(EmoteManagerModule))]
    public class EmoteManagerModule : BotModuleBase
    {
        public EmoteManagerModule(IServiceProvider provider, PaginationService pagination) : base(paginationService: pagination, provider: provider)
        {
        }

        [Command("all")]
        [Summary("Vypíše kompletní statistiku emotů. Řazení se nastavuje pomocí `desc` (Sestupně) a `asc` (Vzestupně). Výchozí je sestupné řazení.")]
        public async Task GetCompleteEmoteInfoListAsync(SortType sortType = SortType.Desc)
        {
            using var service = GetService<EmoteStats>();

            var fields = service.Service.GetAllValues(sortType, Context.Guild.Id, true, EmoteInfoOrderType.Count)
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

        [Command("list")]
        [Summary("Vypíše TOP 25 statistiku emotů. Lze zadat následující možnosti řazení:\n- Desc Count (Sestupně, podle počtu použití) (Výchozí)\n" +
            "- Asc Count (Vzestupně, podle počtu použití)\n- Desc LastUse (Sestupně, podle data posledního použití)\n- Asc LastUse (Vzestupně, podle data posledního použití).")]
        public async Task GetTopEmoteUsage(SortType sortType = SortType.Desc, EmoteInfoOrderType orderType = EmoteInfoOrderType.Count)
        {
            using var service = GetService<EmoteStats>();

            var fields = service.Service.GetAllValues(sortType, Context.Guild.Id, true, orderType)
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID && !x.Animated))
                .Take(EmbedBuilder.MaxFieldCount)
                .Select(o => new EmbedFieldBuilder().WithName(o.RealID).WithValue(o.GetFormatedInfo()));

            var embed = new BotEmbed(Context.Message.Author)
                .WithFields(fields);

            await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("get")]
        [Summary("Statistika emote, nebo uživatele. Pro statistiku uživatele jej musíte označit (tagnout).")]
        public async Task GetEmoteInfoAsync(string emoteOrTag, SortType sortType = SortType.Desc)
        {
            if (Context.Message.MentionedUsers.Any(x => x.Mention == emoteOrTag || $"<@{x.Id}>" == emoteOrTag))
            {
                var user = Context.Message.MentionedUsers.FirstOrDefault(o => o.Mention == emoteOrTag || $"<@{o.Id}>" == emoteOrTag);
                await GetInfoOfUserAsync(user, sortType);
            }
            else
            {
                await GetInfoOfEmoteAsync(emoteOrTag);
            }
        }

        private async Task GetInfoOfEmoteAsync(string emote)
        {
            using var service = GetService<EmoteStats>();

            var existsInGuild = Context.Guild.Emotes.Any(o => o.ToString() == emote);
            var emoteInfo = await service.Service.GetValueAsync(Context.Guild, emote);

            if (emoteInfo == null)
            {
                var bytes = Encoding.Unicode.GetBytes(emote);
                emoteInfo = await service.Service.GetValueAsync(Context.Guild, Convert.ToBase64String(bytes));
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

            if (emoteInfo.TopUsage.Count > 0)
            {
                var leaderboard = new LeaderboardBuilder("Nejpoužívajší uživatelé", Context.User);

                foreach (var usage in emoteInfo.TopUsage)
                {
                    var user = await Context.Guild.GetUserFromGuildAsync(usage.Key);
                    if (user == null) continue;

                    leaderboard.AddItem(user?.GetFullName(), usage.Value.FormatWithSpaces());
                }

                await ReplyAsync(embed: leaderboard.Build());
            }
        }

        private async Task GetInfoOfUserAsync(IUser user, SortType sortType)
        {
            using var service = GetService<EmoteStats>();

            var emotes = (await service.Service.GetEmoteStatsForUserAsync(Context.Guild, user, sortType))
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

            if (emotes.Count == 0)
            {
                await ReplyAsync("Tento uživatel ještě nepoužil žádný emote.");
                return;
            }

            var embed = GetPaginatedResult(emotes, "Kompletní statistika emotů za uživatele");
            await SendPaginatedEmbedAsync(embed);
        }

        [Command("unicode")]
        [Summary("Vypíše TOP25 statistika unicode emojis. Řazení se nastavuje pomocí `desc` (Sestupně) a `asc` (Vzestupně). Výchozí je sestupné řazení.")]
        public async Task GetEmoteInfoOnlyUnicode(SortType sortType = SortType.Desc)
        {
            using var service = GetService<EmoteStats>();

            var fields = service.Service.GetAllUnicodeValues(sortType, Context.Guild.Id)
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
        public async Task ClearOldEmotesAsync()
        {
            using var service = GetService<EmoteStats>();
            var clearedEmotes = await service.Service.CleanOldEmotesAsync(Context.Guild).ConfigureAwait(false);

            await ReplyChunkedAsync(clearedEmotes, 10);
            await ReplyAsync("> Čištění dokončeno.");
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