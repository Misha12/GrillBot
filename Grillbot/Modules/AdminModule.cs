using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config.Dynamic;
using Grillbot.Models.Embed;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [RequirePermissions]
    [Name("Administrační funkce")]
    public class AdminModule : BotModuleBase
    {
        private IMessageCache MessageCache { get; }

        public AdminModule(ConfigRepository config, IMessageCache messageCache) : base(configRepository: config)
        {
            MessageCache = messageCache;
        }

        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(string channel, int takeCount, int skipCount = 0)
        {
            var mentionedChannel = Context.Message.MentionedChannels
                .OfType<SocketTextChannel>()
                .FirstOrDefault(o => $"<#{o.Id}>" == channel);

            if (mentionedChannel != null)
            {
                var pins = await mentionedChannel.GetPinnedMessagesAsync().ConfigureAwait(false);

                if (pins.Count == 0)
                    throw new BotCommandInfoException($"V kanálu **{mentionedChannel.Mention}** ještě nebylo nic připnuto.");

                var pinsToRemove = pins
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip(skipCount).Take(takeCount)
                    .OfType<RestUserMessage>();

                foreach (var pin in pinsToRemove)
                {
                    await pin.RemoveAllReactionsAsync().ConfigureAwait(false);
                    await pin.UnpinAsync().ConfigureAwait(false);
                }

                await ReplyAsync($"Úpěšně dokončeno. Počet odepnutých zpráv: **{pinsToRemove.Count()}**").ConfigureAwait(false);
            }
            else
            {
                throw new BotCommandInfoException($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
            }
        }

        [Command("guildStatus")]
        [Summary("Informace o serveru.")]
        [Remarks("Parametr guildID je povinný v případě volání v soukromé konverzaci.")]
        public async Task GuildStatusAsync(ulong guildID = default)
        {
            var guild = Context.Guild ?? Context.Client.GetGuild(guildID);

            if (guild == null)
            {
                if (guildID == default)
                    throw new ThrowHelpException();

                throw new BotCommandInfoException("Požadovaný server nebyl nalezen.");
            }

            var color = guild.Roles.FindHighestRoleWithColor()?.Color;
            var embed = new BotEmbed(Context.Message.Author, color, title: guild.Name)
                .WithThumbnail(guild.IconUrl)
                .WithFields(
                    new EmbedFieldBuilder().WithName("Počet kategorií").WithValue($"**{guild.CategoryChannels?.Count ?? 0}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet textových kanálů").WithValue($"**{guild.ComputeTextChannelsCount()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet hlasových kanálů").WithValue($"**{guild.ComputeVoiceChannelsCount()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet rolí").WithValue($"**{guild.Roles.Count}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Vytvořen").WithValue($"**{guild.CreatedAt.DateTime.ToLocaleDatetime()}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Vlastník").WithValue($"**{guild.Owner.GetFullName()}** ({guild.OwnerId})"),
                    new EmbedFieldBuilder().WithName("Systémový kanál").WithValue($"**{guild.SystemChannel?.Name ?? "None"}** ({guild.SystemChannel?.Id ?? 0})"),
                    new EmbedFieldBuilder().WithName("Uživatelé synchronizováni").WithValue($"**{(guild.HasAllMembers ? "Ano" : "Ne")}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Synchronizován").WithValue($"**{(guild.IsSynced ? "Ano" : "Ne")}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet uživatelů (v paměti)").WithValue($"**{guild.MemberCount}** (**{guild.Users.Count}**)").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Úroveň ověření").WithValue($"**{guild.VerificationLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("ID oblasti (Hovory)").WithValue($"**{guild.VoiceRegionId ?? "null"}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Úroveň MFA").WithValue($"**{guild.MfaLevel}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Filtr explicitního obsahu").WithValue($"**{guild.ExplicitContentFilter}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Výchozí notifikace").WithValue($"**{guild.DefaultMessageNotifications}**").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Extra funkce").WithValue(guild.Features.Count == 0 ? "-" : string.Join(", ", guild.Features)),
                    new EmbedFieldBuilder().WithName("Tier").WithValue(guild.PremiumTier.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet boosterů").WithValue(guild.PremiumSubscriptionCount).WithIsInline(true)
                );

            await ReplyAsync(embed: embed.Build());
        }

        [Command("syncGuild")]
        [Summary("Synchronizace serveru s botem.")]
        public async Task SyncGuild()
        {
            try
            {
                await Context.Guild.SyncGuildAsync();
                await ReplyAsync("Synchronizace dokončena");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Synchronizace se nezdařila ({ex.Message.PreventMassTags()}).");
                throw;
            }
        }

        [DisabledPM]
        [Command("userinfo")]
        [Summary("Informace o uživateli.")]
        [Remarks("Jako identifikace uživatele může posloužit tag, ID, nebo globální identifikace (User#1234).")]
        public async Task UserInfo(string identification)
        {
            SocketGuildUser user = null;

            if (Context.Message.MentionedUsers.Count > 0)
            {
                user = Context.Message.MentionedUsers.OfType<SocketGuildUser>().FirstOrDefault();
            }
            else
            {
                if (identification.Contains("#"))
                {
                    var nameParts = identification.Split('#');
                    user = await Context.Guild.GetUserFromGuildAsync(nameParts[0], nameParts[1]);
                }
                else
                {
                    try
                    {
                        user = await Context.Guild.GetUserFromGuildAsync(identification);
                    }
                    catch (FormatException)
                    {
                        throw new BotCommandInfoException("Neplatný formát jména. Povolené jsou: ID, Tag, Celý nick (User#1234)");
                    } // Cannot parse user ID.
                }
            }

            UserInfoConfig config = null;
            try { config = GetMethodConfig<UserInfoConfig>("", "userinfo"); }
            catch (ConfigException) { /* There is config optional. */ }

            if (user == null)
                throw new BotCommandInfoException("Takový uživatel na serveru není.");

            var userTopRole = user.Roles.FindHighestRoleWithColor();
            var botRole = config != null ? user.Roles.FirstOrDefault(o => o.Id == config.BotRole) : null;
            var roles = user.Roles.Where(o => !o.IsEveryone).OrderByDescending(o => o.Position).Select(o => o.Name);

            string botRoleText = "Ne";
            if (botRole != null)
            {
                var botRoleMessage = botRole != null ? "Ano (i když discord tvrdí něco jinýho)" : "Ne";
                botRoleText = user.IsUser() ? botRoleMessage : "Ano";
            }

            var embed = new BotEmbed(Context.User, userTopRole?.Color, "Informace o uživateli", user.GetUserAvatarUrl())
                .WithFields(
                    new EmbedFieldBuilder().WithName("ID").WithValue(user.Id).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Jméno").WithValue(user.GetFullName()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Stav").WithValue(user.Status.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Účet založen").WithValue(user.CreatedAt.DateTime.ToLocaleDatetime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Připojen").WithValue(user.JoinedAt.Value.DateTime.ToLocaleDatetime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Bot").WithValue(botRoleText).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Umlčen (Server)").WithValue(user.IsMuted || user.IsDeafened ? "Ano" : "Ne").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Umlčen (Klient)").WithValue(user.IsSelfMuted || user.IsSelfDeafened ? "Ano" : "Ne").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Práva").WithValue(string.Join(", ", user.GuildPermissions.GetPermissionsNames())),
                    new EmbedFieldBuilder().WithName("Role").WithValue(string.Join(", ", roles)),
                    new EmbedFieldBuilder().WithName("Boost od").WithValue(!user.PremiumSince.HasValue ? "Boost nenalezen" : user.PremiumSince.Value.LocalDateTime.ToLocaleDatetime()),
                    new EmbedFieldBuilder().WithName("Aktivní klienti").WithValue(string.Join(", ", user.ActiveClients.Select(o => o.ToString())))
                );

            await ReplyAsync(embed: embed.Build());
        }

        [Command("clear")]
        [Summary("Hromadné mazání zpráv.")]
        public async Task ClearMessagesAsync(int count)
        {
            if (Context.Message.Channel is ITextChannel channel)
            {
                var messages = await channel.GetMessagesAsync(count).FlattenAsync();

                var olderTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays >= 14.0);
                var newerTwoWeeks = messages.Where(o => (DateTime.UtcNow - o.CreatedAt).TotalDays < 14.0);

                await channel.DeleteMessagesAsync(newerTwoWeeks);

                foreach(var oldMessage in olderTwoWeeks)
                {
                    await oldMessage.DeleteAsync();
                }

                MessageCache.TryBulkDelete(messages.Select(o => o.Id));
                
                var message = await ReplyAsync($"Počet smazaných zpráv: {messages.Count()}");
                await Task.Delay(TimeSpan.FromSeconds(10));
                await message.DeleteAsync();
            }
        }
    }
}
