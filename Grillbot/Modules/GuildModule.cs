using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using Grillbot.Services.Permissions.Preconditions;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Group("guild")]
    [Name("Správa discord serveru")]
    [ModuleID("GuildModule")]
    public class GuildModule : BotModuleBase
    {
        [Command("info")]
        [Summary("Informace o serveru")]
        public async Task InfoAsync()
        {
            var guild = Context.Guild;

            var basicEmotesCount = guild.Emotes.Count(o => !o.Animated);
            var animatedCount = guild.Emotes.Count - basicEmotesCount;
            var banCount = (await guild.GetBansAsync()).Count;

            var onlineUsersCount = guild.Users.Count(o => o.Status == UserStatus.Online);
            var offlineUsersCount = guild.Users.Count(o => o.Status == UserStatus.Offline || o.Status == UserStatus.Invisible);
            var idleUsersCount = guild.Users.Count(o => o.Status == UserStatus.Idle || o.Status == UserStatus.AFK);
            var doNotDisturbUsersCount = guild.Users.Count(o => o.Status == UserStatus.DoNotDisturb);

            var color = guild.Roles.FindHighestRoleWithColor()?.Color;
            var embed = new BotEmbed(Context.Message.Author, color, title: guild.Name, thumbnail: guild.IconUrl)
                .AddField("Počet kategorií", (guild.CategoryChannels?.Count ?? 0).FormatWithSpaces(), true)
                .AddField("Počet textových kanálů", guild.ComputeTextChannelsCount().FormatWithSpaces(), true)
                .AddField("Počet hlasových kanálů", guild.ComputeVoiceChannelsCount().FormatWithSpaces(), true)
                .AddField("Počet rolí", guild.Roles.Count.FormatWithSpaces(), true)
                .AddField("Počet běžných emotů", basicEmotesCount.FormatWithSpaces(), true)
                .AddField("Počet animovaných emotů", animatedCount.FormatWithSpaces(), true)
                .AddField("Počet banů", banCount.FormatWithSpaces(), true)
                .AddField("Vytvořen", guild.CreatedAt.DateTime.ToLocaleDatetime(), true)
                .AddField("Vlastník", $"{guild.Owner.GetFullName()} ({guild.OwnerId})", false)
                .AddField("Systémový kanál", $"{guild.SystemChannel?.Name ?? "None"} ({guild.SystemChannel?.Id ?? 0})", false)
                .AddField("Server synchronizován", guild.IsSynced.TranslateToCz(), true)
                .AddField("Počet uživatelů (v paměti)", $"{guild.MemberCount} ({guild.Users.Count})", true)
                .AddField("Úroveň ověření", guild.VerificationLevel.ToString(), true)
                .AddField("Úroveň MFA", guild.MfaLevel.ToString(), true)
                .AddField("Filtr explicitního obsahu", guild.ExplicitContentFilter.ToString(), true)
                .AddField("Výchozí notifikace", guild.DefaultMessageNotifications.ToString(), true)
                .AddField("Extra funkce", guild.Features.Count == 0 ? "-" : string.Join(", ", guild.Features), false)
                .AddField("Tier", guild.PremiumTier.ToString(), true)
                .AddField("Počet boosterů", guild.PremiumSubscriptionCount.FormatWithSpaces(), true)
                .AddField("Stav uživatelů", "_ _", false)
                .AddField("Online", onlineUsersCount.FormatWithSpaces(), true)
                .AddField("Idle", idleUsersCount.FormatWithSpaces(), true)
                .AddField("DoNotDisturb", doNotDisturbUsersCount.FormatWithSpaces(), true)
                .AddField("Offline", offlineUsersCount.FormatWithSpaces(), true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sync")]
        [Summary("Synchronizace serveru")]
        public async Task SyncAsync()
        {
            await Context.Guild.SyncGuildAsync();
            await ReplyAsync("Synchronizace úspěšně dokončena.");
        }
    }
}
