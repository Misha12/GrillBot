using Discord;
using Discord.Commands;
using Grillbot.Services.EmoteStats;
using Grillbot.Services.Preconditions;
using Grillbot.Services.Statistics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [IgnorePM]
    [Name("Správa emotů")]
    [DisabledCheck(RoleGroupName = "EmoteManager")]
    [RequireRoleOrAdmin(RoleGroupName = "EmoteManager")]
    public class EmoteManagerModule : BotModuleBase
    {
        private EmoteStats EmoteStats { get; }

        public EmoteManagerModule(Statistics statistics)
        {
            EmoteStats = statistics.EmoteStats;
        }

        [Command("emoteinfo")]
        [Summary("Statistika emotu")]
        [Remarks("Pokud se místo emotu zadá 'all', tak se vypíší všechny použité emoty.")]
        public async Task GetEmoteInfoAsync(string emote)
        {
            if (emote == "all")
            {
                await GetCompleteEmoteInfoListAsync();
                return;
            }

            if (!Context.Guild.Emotes.Any(o => o.ToString() == emote))
            {
                await ReplyAsync("Neznámý emote");
                return;
            }

            var emoteInfo = EmoteStats.GetValue(emote);

            if (emoteInfo == null)
            {
                await ReplyAsync("Tento emote ještě nebyl použit.");
                return;
            }

            var emoteInfoEmbed = new EmbedBuilder()
                .WithColor(Color.Blue)
                .AddField(o => o.WithName(emoteInfo.EmoteID).WithValue(emoteInfo.GetFormatedInfo()))
                .WithCurrentTimestamp()
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}");

            await ReplyAsync(embed: emoteInfoEmbed.Build());
        }

        public async Task GetCompleteEmoteInfoListAsync()
        {
            var emoteInfos = EmoteStats.GetAllValues()
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID))
                .ToList();

            var embedFields = new List<EmbedFieldBuilder>();
            foreach (var emote in emoteInfos)
            {
                var field = new EmbedFieldBuilder()
                    .WithName(emote.EmoteID)
                    .WithValue(emote.GetFormatedInfo());

                embedFields.Add(field);
            }

            for (int i = 0; i < (float)embedFields.Count / EmbedBuilder.MaxFieldCount; i++)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFields(embedFields.Skip(i * EmbedBuilder.MaxFieldCount).Take(EmbedBuilder.MaxFieldCount))
                    .WithFooter($"Strana {i + 1} | Odpověď pro {GetUsersShortName(Context.Message.Author)}")
                    .WithCurrentTimestamp();

                await ReplyAsync(embed: embed.Build());
            }
        }

        [Command("emoteinfo")]
        [Summary("TOP 25 statistika emotů.")]
        public async Task GetTopUsedEmotes()
        {
            var emoteInfos = EmoteStats.GetAllValues()
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID))
                .Take(EmbedBuilder.MaxFieldCount)
                .ToList();

            var emoteFields = emoteInfos.Select(o => new EmbedFieldBuilder().WithName(o.EmoteID).WithValue(o.GetFormatedInfo())).ToList();

            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithFields(emoteFields)
                .WithFooter($"Odpověď pro {GetUsersShortName(Context.Message.Author)}")
                .WithCurrentTimestamp();

            await ReplyAsync(embed: embedBuilder.Build());
        }
    }
}