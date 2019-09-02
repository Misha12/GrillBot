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
        public async Task GetEmoteInfoAsync(string emote)
        {
            if(!Context.Guild.Emotes.Any(o => o.ToString() == emote))
            {
                await ReplyAsync("Neznámý emote");
                return;
            }

            var emoteInfo = EmoteStats.GetValue(emote);

            if(emoteInfo == null)
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

        [Command("emoteinfo")]
        [Summary("Statistika všech emotů")]
        public async Task GetEmoteInfoAsync()
        {
            var emoteInfos = EmoteStats.GetAllValues()
                .Where(o => Context.Guild.Emotes.Any(x => x.ToString() == o.EmoteID))
                .ToList();

            var embedFields = new List<EmbedFieldBuilder>();
            foreach(var emote in emoteInfos)
            {
                var field = new EmbedFieldBuilder()
                    .WithName(emote.EmoteID)
                    .WithValue(emote.GetFormatedInfo());

                embedFields.Add(field);
            }

            for(int i = 0; i < (float)embedFields.Count / EmbedBuilder.MaxFieldCount; i++)
            {
                var embed = new EmbedBuilder()
                    .WithColor(Color.Blue)
                    .WithFields(embedFields.Skip(i * EmbedBuilder.MaxFieldCount).Take(EmbedBuilder.MaxFieldCount))
                    .WithFooter($"Strana {i} | Odpověď pro {GetUsersShortName(Context.Message.Author)}")
                    .WithCurrentTimestamp();

                await ReplyAsync(embed: embed.Build());
            }
        }
    }
}
