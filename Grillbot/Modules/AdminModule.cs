using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed;
using Grillbot.Services.AdminServices;
using Grillbot.Services.MessageCache;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
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
        private PinManagement PinManagement { get; }
        private UserService UserService { get; }

        public AdminModule(ConfigRepository config, IMessageCache messageCache, PinManagement pinManagement,
            UserService userService) : base(configRepository: config)
        {
            MessageCache = messageCache;
            PinManagement = pinManagement;
            UserService = userService;
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
                await PinManagement.PinPurgeAsync(mentionedChannel, takeCount, skipCount);
                return;
            }

            await ReplyAsync($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
        }

        [DisabledPM]
        [Command("guildStatus")]
        [Summary("Informace o serveru.")]
        public async Task GuildStatusAsync()
        {
            var guild = Context.Guild;

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
            var user = await Context.ParseGuildUserAsync(identification);

            if (user == null)
            {
                await ReplyAsync("Neplatné jméno, nebo uživatel na serveru není. Povolené jsou: ID, Tag, Celý nick (User#1234)");
                return;
            }

            var userTopRoleWithColor = user.Roles.FindHighestRoleWithColor();

            var roles = user.Roles
                .Where(o => !o.IsEveryone)
                .OrderByDescending(o => o.Position)
                .Select(o => o.Name);

            var userDetail = await UserService.GetUserAsync(Context.Guild, user);

            var embed = new BotEmbed(Context.User, userTopRoleWithColor?.Color, "Informace o uživateli", user.GetUserAvatarUrl())
                .WithFields(
                    new EmbedFieldBuilder().WithName("ID").WithValue(user.Id).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Jméno").WithValue(user.GetFullName()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Stav").WithValue(user.Status.ToString()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Účet založen").WithValue(user.CreatedAt.DateTime.ToLocaleDatetime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Připojen").WithValue(user.JoinedAt.Value.DateTime.ToLocaleDatetime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Umlčen (Server)").WithValue((user.IsMuted || user.IsDeafened).TranslateToCz()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Umlčen (Klient)").WithValue((user.IsSelfMuted || user.IsSelfDeafened).TranslateToCz()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Práva").WithValue(string.Join(", ", user.GuildPermissions.GetPermissionsNames())),
                    new EmbedFieldBuilder().WithName("Role").WithValue(string.Join(", ", roles)),
                    new EmbedFieldBuilder().WithName("Boost od").WithValue(!user.PremiumSince.HasValue ? "Boost nenalezen" : user.PremiumSince.Value.LocalDateTime.ToLocaleDatetime()).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Aktivní klienti").WithValue(string.Join(", ", user.ActiveClients.Select(o => o.ToString()))).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Body").WithValue(FormatHelper.FormatWithSpaces(userDetail?.Points ?? 0)).WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Reakce").WithValue(userDetail?.FormatReactions() ?? "0 / 0").WithIsInline(true),
                    new EmbedFieldBuilder().WithName("Počet zpráv").WithValue(FormatHelper.FormatWithSpaces(userDetail?.TotalMessageCount ?? 0)).WithIsInline(true)
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

                foreach (var oldMessage in olderTwoWeeks)
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
