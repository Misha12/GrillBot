using Discord;
using Discord.Commands;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Embed;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
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

        [Command("calc_perms")]
        [Summary("Spočítá počet oprávnění v kanálu/na serveru.")]
        public async Task CalcPermsAsync(IGuildChannel channel = null)
        {
            await ReplyAsync("Probíhá výpočet oprávnění. Tato operace může několik minut trvat.");

            await Context.Guild.SyncGuildAsync();
            var channels = new List<IGuildChannel>();
            if (channel == null) channels.AddRange(Context.Guild.Channels);
            else channels.Add(channel);

            uint totalUserPermsCount = 0;
            uint totalModPermsCount = 0;

            foreach (var guildChannel in channels)
            {
                foreach (var user in Context.Guild.Users)
                {
                    var perm = guildChannel.GetPermissionOverwrite(user);

                    if (perm != null)
                    {
                        totalUserPermsCount++;

                        if (user.GuildPermissions.Administrator)
                            totalModPermsCount++;
                    }
                }
            }

            await ReplyAsync($"Výpočet práv dokončen.\nCelkem oprávnění: **{totalUserPermsCount.FormatWithSpaces()}** z toho moderátorských: **{totalModPermsCount.FormatWithSpaces()}**");
        }

        [Command("clear_perms")]
        [Summary("Smaže všechny uživatelské oprávnění v kanálu.")]
        public async Task ClearPermsAsync(bool onlyMod, IGuildChannel guildChannel = null)
        {
            await Context.Guild.SyncGuildAsync();

            foreach (var channel in Context.Guild.Channels.Where(o => guildChannel == null || o.Id == guildChannel.Id))
            {
                foreach (var user in Context.Guild.Users.Where(o => !onlyMod || o.GuildPermissions.Administrator))
                {
                    var perm = channel.GetPermissionOverwrite(user);

                    if (perm != null)
                    {
                        await channel.RemovePermissionOverwriteAsync(user);
                        await ReplyAsync($"Smazáno oprávnění z kanálu `{channel}` pro uživatele {user.GetFullName()}");
                    }
                }
            }

            await ReplyAsync("Úklid oprávnění v kanálu dokončeno.");
        }
    }
}
