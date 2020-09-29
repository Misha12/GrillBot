using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("user")]
    [ModuleID("UserModule")]
    [Name("Správa uživatelů")]
    public class UserModule : BotModuleBase
    {
        private UserService UserService { get; }

        public UserModule(UserService userService, PaginationService pagination) : base(paginationService: pagination)
        {
            UserService = userService;
        }

        [Command("generateApiToken")]
        [Summary("Vygenerování přístupového tokenu k REST API pro uživatele.")]
        [Remarks("Pokud uživatel již přístup k API měl, tak zavolání příkazu mu vygeneruje nový token a starý zneplatní.")]
        public async Task GenerateApiToken(SocketUser userMention)
        {
            var token = UserService.GenerateApiToken(Context.Guild, userMention);
            await userMention.SendPrivateMessageAsync($"Byl ti vygenerován nový token pro přístup k REST API.\nTvůj token je: `{token}`");
            await ReplyAsync("Token vygenerován a odeslán do PM");
        }

        [Command("releaseApiToken")]
        [Summary("Uvolnění vygenerovaného tokenu k REST API.")]
        public async Task RelaseApiToken(SocketUser userMention)
        {
            try
            {
                UserService.ReleaseApiToken(Context.Guild, userMention);
                await ReplyAsync("Token byl z databáze uvolněn.");
            }
            catch (ValidationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("addToWebAdmin")]
        [Summary("Udělení přístupu uživatele do webové administrace.")]
        public async Task AddUserToWebAdmin(IUser userMention)
        {
            try
            {
                var password = UserService.AddUserToWebAdmin(Context.Guild, (SocketGuildUser)userMention);

                await userMention.SendPrivateMessageAsync(
                    $"Byl ti udělen přístup do webové administrace. Uživatelské jméno je tvůj globální discord nick.\nHeslo máš zde: `{password}`. Uchovej si ho.");
                await ReplyAsync("Přístup umožněn.");
            }
            catch (InvalidOperationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("removeFromWebAdmin")]
        [Summary("Odebrání uživatele z webové administrace.")]
        public async Task RemoveUser(IUser userMention)
        {
            try
            {
                UserService.RemoveUserFromWebAdmin(Context.Guild, (SocketGuildUser)userMention);
                await ReplyAsync("Přístup byl odebrán.");
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("info")]
        [Summary("Informace o konkrétním uživateli.")]
        public async Task InfoAsync(IUser userMention)
        {
            var user = userMention is SocketGuildUser usr ? usr : await Context.Guild.GetUserFromGuildAsync(userMention.Id);

            if (user == null)
            {
                await ReplyAsync("Uživatel nebyl na tomto serveru nalezen.");
                return;
            }

            var userDetail = await UserService.GetUserInfoAsync(Context.Guild, user, true);

            if (userDetail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveru, nebo neprojevil aktivitu.");
                return;
            }

            var mostActiveChannel = userDetail.GetMostActiveChannel();
            var lastActiveChannel = userDetail.GetLastActiveChannel();

            var detailFlags = userDetail.GetDetailFlags();
            var clients = userDetail.User.ActiveClients.Select(o => o.ToString());

            var embed = await UserInfoHelper.CreateSimpleEmbedAsync(userDetail, Context);
            embed
                .AddField("Práva", string.Join(", ", userDetail.User.GuildPermissions.GetPermissionsNames()), false);

            if (clients.Any())
                embed.AddField("Aktivní klienti", string.Join(", ", clients), false);

            if (mostActiveChannel != null)
                embed.AddField("Nejaktivnější kanál", $"#{mostActiveChannel.Channel.Name} ({mostActiveChannel.Count.FormatWithSpaces()})", false);

            if (lastActiveChannel != null)
                embed.AddField("Poslední zpráva v", $"#{lastActiveChannel.Channel.Name} ({lastActiveChannel.LastMessageAt.ToLocaleDatetime()})", false);

            embed
                .AddField("Detaily", detailFlags.Count == 0 ? "-" : string.Join(", ", detailFlags), false);

            if (userDetail.Statistics != null)
            {
                embed
                    .AddField("Počet volání API", userDetail.Statistics.ApiCallsCount.FormatWithSpaces(), true)
                    .AddField("Počet přihlášení", userDetail.Statistics.WebAdminLoginCount.FormatWithSpaces(), true);
            }

            await ReplyAsync(embed: embed.Build());
        }

        [Command("access")]
        [Summary("Zjištění, kam zadaný uživatel vidí.")]
        public async Task GetAccessListAsync(IUser user)
        {
            var guildUser = user as SocketGuildUser;
            await Context.Guild.SyncGuildAsync();

            var textChannels = Context.Guild.TextChannels
                .OrderBy(o => o.Position)
                .Where(o => o.HaveAccess(guildUser))
                .GroupBy(o => o.Category?.Name ?? "Neznámá kategorie")
                .Select(o => new { Category = o.Key, Channels = o.SplitInParts(30).Select(x => x.Select(t => $"<#{t.Id}>")) });

            var voiceChannels = Context.Guild.VoiceChannels
                .OrderBy(o => o.Position)
                .Where(o => o.HaveAccess(guildUser))
                .GroupBy(o => o.Category?.Name ?? "Neznámá kategorie")
                .Select(o => new { Category = o.Key, Channels = o.SplitInParts(30).Select(x => x.Select(t => $"<#{t.Id}>")) });

            var finalList = textChannels.ToList();
            finalList.AddRange(voiceChannels);

            var pages = finalList.SplitInParts(10).Select(o =>
            {
                var fields = o.SelectMany(x => x.Channels.Select(t => new EmbedFieldBuilder().WithName(x.Category).WithValue(string.Join(", ", t)))).ToList();
                return new PaginatedEmbedPage(null, fields);
            });

            var embed = new PaginatedEmbed()
            {
                Color = guildUser.Roles.FindHighestRoleWithColor()?.Color,
                Pages = pages.ToList(),
                ResponseFor = Context.User,
                Thumbnail = user.GetUserAvatarUrl(),
                Title = $"Seznam přístupů uživatele {user.GetFullName()}"
            };

            await SendPaginatedEmbedAsync(embed);
        }
    }
}
