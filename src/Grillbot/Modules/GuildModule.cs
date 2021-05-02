using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed;
using Grillbot.Models.Users;
using Grillbot.Services.BackgroundTasks;
using Grillbot.Services.Unverify;
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
        private BackgroundTaskQueue Queue { get; }

        public GuildModule(BackgroundTaskQueue queue)
        {
            Queue = queue;
        }

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
                .AddField("Počet emotů (běžných/animovaných)", $"{basicEmotesCount.FormatWithSpaces()} / {animatedCount.FormatWithSpaces()}", true)
                .AddField("Počet banů", banCount.FormatWithSpaces(), true)
                .AddField("Vytvořen", guild.CreatedAt.DateTime.ToLocaleDatetime(), true)
                .AddField("Vlastník", $"{guild.Owner.GetFullName()} ({guild.OwnerId})", false)
                .AddField("Server synchronizován", guild.IsSynced.TranslateToCz(), true)
                .AddField("Počet uživatelů (v paměti)", $"{guild.MemberCount} ({guild.Users.Count})", true)
                .AddField("Tier", guild.PremiumTier.ToString(), true)
                .AddField("Počet boosterů", guild.PremiumSubscriptionCount.FormatWithSpaces(), true)
                .AddField("Extra funkce", guild.Features.Count == 0 ? "-" : string.Join(", ", guild.Features), false)
                .AddField("Stav uživatelů", "_ _", false)
                .AddField("Online", onlineUsersCount.FormatWithSpaces(), true)
                .AddField("Idle", idleUsersCount.FormatWithSpaces(), true)
                .AddField("DoNotDisturb", doNotDisturbUsersCount.FormatWithSpaces(), true)
                .AddField("Offline", offlineUsersCount.FormatWithSpaces(), true)
                .AddField("Limity", "_ _", false)
                .AddField("Max. uživatelů", guild.MaxMembers?.FormatWithSpaces() ?? "Není známo", true)
                .AddField("Max. online uživatelů", guild.MaxPresences?.FormatWithSpaces() ?? "Není známo", true)
                .AddField("Max. uživatelů s webkou", guild.MaxVideoChannelUsers?.FormatWithSpaces() ?? "Není známo", true)
                .AddField("Max. bitrate", $"{guild.MaxBitrate.FormatWithSpaces()} kbps", true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("sync")]
        [Summary("Synchronizace serveru")]
        public async Task SyncAsync()
        {
            await Context.Guild.SyncGuildAsync();
            await ReplyAsync("Synchronizace úspěšně dokončena.");
        }

        [Command("calcPerms")]
        [Summary("Spočítá počet oprávnění v kanálu/na serveru.")]
        public async Task CalcPermsAsync(bool verbose = false, IGuildChannel channel = null)
        {
            await ReplyAsync("Probíhá výpočet oprávnění. Tato operace může několik minut trvat.");

            await Context.Guild.SyncGuildAsync();
            var channels = new List<IGuildChannel>();
            if (channel == null)
            {
                channels.AddRange(Context.Guild.VoiceChannels);
                channels.AddRange(Context.Guild.TextChannels);
                channels.AddRange(Context.Guild.CategoryChannels);
            }
            else
            {
                channels.Add(channel);
            }

            var channelsInfo = new Dictionary<IGuildChannel, uint>();
            foreach (var guildChannel in channels)
            {
                uint permsCount = 0;

                foreach (var user in Context.Guild.Users)
                {
                    var perm = guildChannel.GetPermissionOverwrite(user);

                    if (perm != null)
                        permsCount++;
                }

                if (permsCount > 0)
                    channelsInfo.Add(guildChannel, permsCount);
            }

            if (verbose)
            {
                var chunks = channelsInfo
                    .OrderByDescending(o => o.Value)
                    .Select(o => $"> <#{o.Key.Id}> ({o.Key.GetType().Name.Replace("Socket", "").Replace("Channel", "")}), celkem práv: **{o.Value.FormatWithSpaces()}**")
                    .SplitInParts(5);

                await ReplyChunkedAsync(chunks);
            }

            var totalPermsCount = channelsInfo.Sum(o => o.Value);
            await ReplyAsync($"Výpočet práv dokončen.\nCelkem oprávnění: **{totalPermsCount.FormatWithSpaces()}**.");
        }

        [Command("clearPerms")]
        [Summary("Smaže všechny uživatelské oprávnění v kanálu.")]
        public async Task ClearPermsAsync(bool onlyMod, IGuildChannel guildChannel = null)
        {
            var msg = await ReplyAsync("Příprava čištění oprávnění.");
            await Context.Guild.SyncGuildAsync();

            var allChannels = new List<SocketGuildChannel>();
            allChannels.AddRange(Context.Guild.CategoryChannels);
            allChannels.AddRange(Context.Guild.TextChannels);
            allChannels.AddRange(Context.Guild.VoiceChannels);

            uint clearedPerms = 0;
            foreach (var channel in allChannels.Where(o => guildChannel == null || o.Id == guildChannel.Id))
            {
                foreach (var user in Context.Guild.Users.Where(o => !onlyMod || o.GuildPermissions.Administrator))
                {
                    var perm = channel.GetPermissionOverwrite(user);

                    if (perm != null)
                    {
                        await channel.RemovePermissionOverwriteAsync(user);
                        clearedPerms++;

                        if (clearedPerms == 1 || clearedPerms % 4 == 0)
                            await msg.ModifyAsync(o => o.Content = $"Probíhá mazání oprávnění z kanálu <#{channel.Id}>. Smazáno práv: {clearedPerms.FormatWithSpaces()}");
                    }
                }
            }

            await ReplyAsync("Úklid oprávnění v kanálu dokončeno.");
        }

        [Command("clearReact")]
        [Summary("Smaže všechny reakce pro emote.")]
        public async Task ClearReactAsync(SocketTextChannel channel, ulong messageId, string react)
        {
            await Context.Guild.SyncGuildAsync();
            var message = await channel.GetMessageAsync(messageId);

            if (message == null)
            {
                await ReplyAsync("Hledaná zpráva neexistuje.");
                return;
            }

            var reaction = message.Reactions.SingleOrDefault(o => o.Key.Name.Contains(react));

            if (reaction.Key == null)
            {
                await ReplyAsync("Hledaná reakce neexistuje.");
                return;
            }

            await message.RemoveAllReactionsForEmoteAsync(reaction.Key);
            await Context.Message.AddReactionAsync(EmojiHelper.OKEmoji);
        }

        [Command("clearUselessPerms")]
        [Summary("Smaže zbytečná uživatelská oprávnění z kanálů.")]
        public async Task ClearUselessPerms(bool hard = false, IUser mentionedUser = null)
        {
            await ReplyAsync("Probíhá příprava...");
            await Context.Guild.SyncGuildAsync();

            // List of channels and users to clear.
            var toClear = new Dictionary<ulong, List<UselessPermData>>();

            var toDetectUsers = new List<SocketGuildUser>();
            if (mentionedUser != null) toDetectUsers.Add(await mentionedUser.ConvertToGuildUserAsync(Context.Guild));
            else toDetectUsers.AddRange(Context.Guild.Users);

            await ReplyAsync("Probíhá výpočet zbytečných oprávnění...");
            foreach (var channel in Context.Guild.Channels.Where(o => o is SocketTextChannel || o is SocketVoiceChannel).OrderBy(o => o.Position))
            {
                foreach (var user in toDetectUsers)
                {
                    if (Queue.Exists<UnverifyBackgroundTask>(o => o.GuildId == Context.Guild.Id && o.UserId == user.Id))
                        continue; // Ignore unverify/selfunverify.

                    var overwrite = channel.GetPermissionOverwrite(user);

                    // User not have overwrite.
                    if (overwrite == null)
                        continue;

                    // User have overwrite without extra perms. It's useless.
                    if (overwrite.Value.AllowValue == 0 && overwrite.Value.DenyValue == 0)
                    {
                        if (toClear.ContainsKey(channel.Id))
                            toClear[channel.Id].Add(new UselessPermData(user, true, false));
                        else
                            toClear.Add(channel.Id, new List<UselessPermData>() { new UselessPermData(user, true, false) });

                        continue;
                    }

                    // Iterate only over user roles.
                    foreach (var role in user.Roles.OrderByDescending(o => o.Position))
                    {
                        // Find highest role that is defined in overwrites and user it have.
                        var roleOverwrite = channel.GetPermissionOverwrite(role);

                        if (roleOverwrite == null)
                            continue;

                        // User have something extra.
                        if (roleOverwrite.Value.AllowValue != overwrite.Value.AllowValue || roleOverwrite.Value.DenyValue != overwrite.Value.DenyValue)
                            break;

                        if (toClear.ContainsKey(channel.Id))
                            toClear[channel.Id].Add(new UselessPermData(user, false, true));
                        else
                            toClear.Add(channel.Id, new List<UselessPermData>() { new UselessPermData(user, false, true) });
                        break;
                    }
                }
            }

            if (hard)
            {
                foreach (var channelPair in toClear)
                {
                    var channel = Context.Guild.GetChannel(channelPair.Key);

                    if (channel == null)
                        continue;

                    var perms = channelPair.Value.Where(o => mentionedUser == null || o.User.Id == mentionedUser.Id);
                    await ReplyAsync($"V kanálu `{channel.Name}` bude smazáno oprávnění: {perms.Count().FormatWithSpaces()}");

                    foreach (var user in perms)
                    {
                        await ReplyAsync($"> Mažu oprávnění z kanálu `{channel.Name}` pro uživatele {user.User.GetFullName()}");
                        await channel.RemovePermissionOverwriteAsync(user.User);
                    }
                }
            }
            else
            {
                await ReplyAsync($"Nalezeno k pročištění kanálů: **{toClear.Count.FormatWithSpaces()}**");

                var totalNeutral = toClear.Sum(o => o.Value.Count(o => o.NeutralUseless));
                var totalAllowDeny = toClear.Sum(o => o.Value.Count(o => o.AllowDenyUseless));

                await ReplyAsync($"V těchto kanálech je: Neutrálních: **{totalNeutral.FormatWithSpaces()}**, Povolení/Zakázání: **{totalAllowDeny.FormatWithSpaces()}**");

                foreach (var channelPair in toClear)
                {
                    var channel = Context.Guild.GetChannel(channelPair.Key);
                    var neutralCount = channelPair.Value.Count(o => o.NeutralUseless);
                    var allowDenyCount = channelPair.Value.Count(o => o.AllowDenyUseless);

                    await ReplyAsync($"> Kanál: `{channel.Name}`: Neutrálních: **{neutralCount.FormatWithSpaces()}**, " +
                        $"Povolení/Zakázání: **{allowDenyCount.FormatWithSpaces()}**");
                }
            }

            await ReplyAsync("Dokončeno");
            await Context.Message.AddReactionAsync(EmojiHelper.OKEmoji);
        }
    }
}
