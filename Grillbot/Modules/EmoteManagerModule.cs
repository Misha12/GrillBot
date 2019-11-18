using Discord;
using Discord.Commands;
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
    [Name("Správa emotů")]
    [RequirePermissions("EmoteManager", DisabledForPM = true, BoosterAllowed = true)]
    public class EmoteManagerModule : BotModuleBase
    {
        private EmoteStats EmoteStats { get; }

        public EmoteManagerModule(Statistics statistics)
        {
            EmoteStats = statistics.EmoteStats;
        }

        [Command("all")]
        [Summary("Vypíše kompletní statistiku emotů. Seřazeno vzestupně.")]
        public async Task GetCompleteEmoteInfoListAsync()
        {
            var emoteInfos = EmoteStats.GetAllValues(true)
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID))
                .ToList();

            var embedFields = new List<EmbedFieldBuilder>();
            foreach (var emote in emoteInfos)
            {
                var field = new EmbedFieldBuilder()
                    .WithName(emote.GetRealId())
                    .WithValue(emote.GetFormatedInfo());

                embedFields.Add(field);
            }

            for (int i = 0; i < (float)embedFields.Count / EmbedBuilder.MaxFieldCount; i++)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFields(embedFields.Skip(i * EmbedBuilder.MaxFieldCount).Take(EmbedBuilder.MaxFieldCount))
                    .WithFooter($"Strana {i + 1} | Odpověď pro {GetUsersShortName(Context.Message.Author)}", GetUserAvatarUrl(Context.Message.Author))
                    .WithCurrentTimestamp();

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("desc")]
        [Summary("TOP 25 statistika emotů. Seřazeno sestupně.")]
        public async Task GetTopUsedEmotes() => await GetTopEmoteUsage(true);

        [Command("asc")]
        [Summary("TOP 25 statistika emotů. Seřazeno vzestupně.")]
        public async Task GetTopUsedEmotesAscending() => await GetTopEmoteUsage(false);

        private async Task GetTopEmoteUsage(bool descOrder)
        {
            var emoteInfos = EmoteStats.GetAllValues(descOrder)
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID && !x.Animated))
                .Take(EmbedBuilder.MaxFieldCount)
                .ToList();

            var emoteFields = emoteInfos.Select(o => new EmbedFieldBuilder().WithName(o.GetRealId()).WithValue(o.GetFormatedInfo())).ToList();

            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithFields(emoteFields)
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}", GetUserAvatarUrl(Context.Message.Author))
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embedBuilder.Build());
        }

        [Command("")]
        [Summary("Statistika emotu")]
        [Remarks("Parametr 'all' vypíše všechno. Parametr 'asc' vypíše TOP25 vzestupně.  Parametr 'desc' vypšíše TOP25 sestupně.")]
        public async Task GetEmoteInfoAsync([Remainder] string emote)
        {
            switch (emote)
            {
                case "all":
                    await GetCompleteEmoteInfoListAsync();
                    return;
                case "asc":
                    await GetTopUsedEmotesAscending();
                    return;
                case "desc":
                    await GetTopUsedEmotes();
                    return;
                case "unicode":
                    await GetEmoteInfoOnlyUnicode();
                    return;
            }

            var existsInGuild = Context.Guild.Emotes.Any(o => o.ToString() == emote);
            var emoteInfo = EmoteStats.GetValue(emote);

            if(emoteInfo == null)
            {
                var bytes = Encoding.Unicode.GetBytes(emote);
                emoteInfo = EmoteStats.GetValue(Convert.ToBase64String(bytes));
            }

            if(emoteInfo == null)
            {
                if(!existsInGuild)
                {
                    await ReplyAsync("Tento emote neexistuje.");
                    return;
                }

                await ReplyAsync("Tento emote ještě nebyl použit.");
                return;
            }

            var emoteInfoEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .AddField(o => o.WithName(emoteInfo.GetRealId()).WithValue(emoteInfo.GetFormatedInfo()))
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}", GetUserAvatarUrl(Context.Message.Author));

            await ReplyAsync(embed: emoteInfoEmbed.Build());
        }

        [Command("unicode")]
        [Summary("Vypíše TOP25 statistika unicode emojis.")]
        private async Task GetEmoteInfoOnlyUnicode()
        {
            var emoteInfos = EmoteStats.GetAllValues(true)
                .Where(o => o.IsUnicode)
                .Take(EmbedBuilder.MaxFieldCount)
                .ToList();

            var emoteFields = emoteInfos.Select(o => new EmbedFieldBuilder().WithName(o.GetRealId()).WithValue(o.GetFormatedInfo())).ToList();

            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithFields(emoteFields)
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}", GetUserAvatarUrl(Context.Message.Author))
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}