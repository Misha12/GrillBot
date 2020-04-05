using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Models.Config;
using Grillbot.Models.Embed;
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
        public AdminModule(ConfigRepository config) : base(configRepository: config) { }

        [Command("pinpurge")]
        [Summary("Hromadné odpinování zpráv.")]
        [Remarks("Poslední parametr skipCount je volitelný. Výchozí hodnota je 0.")]
        public async Task PinPurge(string channel, int takeCount, int skipCount = 0)
        {
            await DoAsync(async () =>
            {
                var mentionedChannel = Context.Message.MentionedChannels
                    .OfType<SocketTextChannel>()
                    .FirstOrDefault(o => $"<#{o.Id}>" == channel);

                if (mentionedChannel != null)
                {
                    var pins = await mentionedChannel.GetPinnedMessagesAsync().ConfigureAwait(false);

                    if (pins.Count == 0)
                        throw new ArgumentException($"V kanálu **{mentionedChannel.Mention}** ještě nebylo nic připnuto.");

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
                    throw new ArgumentException($"Odkazovaný textový kanál **{channel}** nebyl nalezen.");
                }
            }).ConfigureAwait(false);
        }

        [Command("guildStatus")]
        [Summary("Informace o serveru.")]
        [Remarks("Parametr guildID je povinný v případě volání v soukromé konverzaci.")]
        public async Task GuildStatusAsync(ulong guildID = default)
        {
            await DoAsync(async () =>
            {
                var guild = Context.Guild ?? Context.Client.GetGuild(guildID);

                if (guild == null)
                {
                    if (guildID == default)
                        throw new ThrowHelpException();

                    throw new ArgumentException("Požadovaný server nebyl nalezen.");
                }

                var color = guild.Roles.OrderByDescending(o => o.Position).FirstOrDefault()?.Color;
                var embed = new BotEmbed(Context.Message.Author, color, title: guild.Name)
                    .WithThumbnail(guild.IconUrl)
                    .WithFields(
                        new EmbedFieldBuilder().WithName("Počet kategorií").WithValue($"**{guild.CategoryChannels?.Count ?? 0}**").WithIsInline(true),
                        new EmbedFieldBuilder().WithName("Počet kanálů").WithValue($"**{guild.Channels.Count}**").WithIsInline(true),
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
                        new EmbedFieldBuilder().WithName("Výchozí notifikace").WithValue($"**{guild.DefaultMessageNotifications}**").WithIsInline(true)
                    );

                await ReplyAsync(embed: embed.Build());
            });
        }

        [Command("syncGuild")]
        [Summary("Synchronizace serveru s botem.")]
        public async Task SyncGuild()
        {
            try
            {
                await Context.Guild.SyncGuildAsync().ConfigureAwait(false);
                await ReplyAsync("Synchronizace dokončena").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Synchronizace se nezdařila ({ex.Message.PreventMassTags()}).").ConfigureAwait(false);
                throw;
            }
        }

        [DisabledPM]
        [Command("userinfo")]
        [Summary("Informace o uživateli.")]
        [Remarks("Jako identifikace uživatele může posloužit tag, ID, nebo globální identifikace (User#1234).")]
        public async Task UserInfo(string identification)
        {
            await DoAsync(async () =>
            {
                SocketGuildUser user = null;

                if (Context.Message.MentionedUsers.Count > 0)
                    user = Context.Message.MentionedUsers.OfType<SocketGuildUser>().FirstOrDefault();
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
                        catch (FormatException) { return; } // Cannot parse user ID.
                    }
                }

                UserInfoConfig config = null;
                try { config = GetMethodConfig<UserInfoConfig>("", "userinfo"); }
                catch (ConfigException) { /* There is config optional. */ }

                if (user == null)
                    throw new ArgumentException("Takový uživatel na serveru není.");

                var userTopRole = user.FindHighestRoleWithColor();
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
                        new EmbedFieldBuilder().WithName("Role").WithValue(string.Join(", ", roles))
                    );

                await ReplyAsync(embed: embed.Build());
            });
        }
    }
}
