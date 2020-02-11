using Discord;
using Discord.Commands;
using Grillbot.Models.Embed;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Statistics;
using System;
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

        public EmoteManagerModule(EmoteStats emoteStats)
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

            const int maxFieldsCount = EmbedBuilder.MaxFieldCount;
            var pagesCount = Math.Ceiling((float)fields.Count / maxFieldsCount);
            for (int i = 0; i < pagesCount; i++)
            {
                var embed = new BotEmbed(Context.Message.Author)
                    .PrependFooter($"Strana {i + 1} z {pagesCount}")
                    .WithFields(fields.Skip(i * maxFieldsCount).Take(maxFieldsCount));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("desc")]
        [Summary("TOP 25 statistika emotů. Seřazeno sestupně.")]
        public async Task GetTopUsedEmotes() => await GetTopEmoteUsage(true).ConfigureAwait(false);

        [Command("asc")]
        [Summary("TOP 25 statistika emotů. Seřazeno vzestupně.")]
        public async Task GetTopUsedEmotesAscending() => await GetTopEmoteUsage(false).ConfigureAwait(false);

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

        [Command("")]
        [Summary("Statistika emotu")]
        [Remarks("Parametr 'all' vypíše všechno. Parametr 'asc' vypíše TOP25 vzestupně.  Parametr 'desc' vypšíše TOP25 sestupně.")]
        public async Task GetEmoteInfoAsync([Remainder] string emote)
        {
            switch (emote)
            {
                case "all":
                    await GetCompleteEmoteInfoListAsync().ConfigureAwait(false);
                    return;
                case "asc":
                    await GetTopUsedEmotesAscending().ConfigureAwait(false);
                    return;
                case "desc":
                    await GetTopUsedEmotes().ConfigureAwait(false);
                    return;
                case "unicode":
                    await GetEmoteInfoOnlyUnicode().ConfigureAwait(false);
                    return;
                case "emoteMergeList":
                    await GetMergeListAsync().ConfigureAwait(false);
                    return;
                case "processEmoteMerge":
                    await ProcessEmoteMergeAsync().ConfigureAwait(false);
                    return;
                case "cleanOldEmotes":
                    await CleanOldEmotes().ConfigureAwait(false);
                    return;
            }

            await DoAsync(async () =>
            {
                var existsInGuild = Context.Guild.Emotes.Any(o => o.ToString() == emote);
                var emoteInfo = EmoteStats.GetValue(emote);

                if (emoteInfo == null)
                {
                    var bytes = Encoding.Unicode.GetBytes(emote);
                    emoteInfo = EmoteStats.GetValue(Convert.ToBase64String(bytes));
                }

                if (emoteInfo == null)
                {
                    if (!existsInGuild)
                        throw new ArgumentException("Tento emote neexistuje");

                    throw new ArgumentException("Tento emote ještě nebyl použit");
                }

                var embed = new BotEmbed(Context.Message.Author)
                    .AddField(o => o.WithName(emoteInfo.GetRealId()).WithValue(emoteInfo.GetFormatedInfo()));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
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
            await DoAsync(async () =>
            {
                var list = EmoteStats.GetMergeList(Context.Guild);

                if (list.Count == 0)
                    throw new ArgumentException("Aktuálně není nic ke sloučení.");

                var embed = new BotEmbed(Context.Message.Author, title: "Seznam potenciálních sloučení emotů");

                embed.WithFields(list.Select(o => new EmbedFieldBuilder()
                {
                    Name = $"Target: \\{o.MergeTo}",
                    Value = $"Sources: {Environment.NewLine}{string.Join(Environment.NewLine, o.Emotes.Select(x => $"[\\{x.Key}, {x.Value}]"))}"
                }));

                await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("processEmoteMerge")]
        [Summary("Provede sloučení stejných emotů ve statistikách.")]
        public async Task ProcessEmoteMergeAsync()
        {
            await DoAsync(async () =>
            {
                await EmoteStats.MergeEmotesAsync(Context.Guild).ConfigureAwait(false);
                await ReplyAsync("Sloučení dokončeno").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [Command("cleanOldEmotes")]
        public async Task CleanOldEmotes()
        {
           await DoAsync(async () =>
           {
               var clearedEmotes = await EmoteStats.CleanOldEmotesAsync(Context.Guild).ConfigureAwait(false);

               for(int i = 0; i < Math.Ceiling(clearedEmotes.Count / 10.0); i++)
               {
                   var str = string.Join("\n", clearedEmotes.Skip(i * 10).Take(10));
                   await ReplyAsync(str).ConfigureAwait(false);
               }

               await ReplyAsync("Čištění dokončeno.").ConfigureAwait(false);
           }).ConfigureAwait(false);
        }
    }
}