using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Attributes;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Helpers;
using Grillbot.Models.Embed.PaginatedEmbed;
using Grillbot.Services;
using Grillbot.Services.UserManagement;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("user")]
    [Name("Správa uživatelů")]
    [ModuleID(nameof(UserModule))]
    public class UserModule : BotModuleBase
    {
        public UserModule(PaginationService pagination, IServiceProvider provider) : base(pagination, provider)
        {
        }

        [Command("generateApiToken")]
        [Summary("Vygenerování přístupového tokenu k REST API pro uživatele.")]
        [Remarks("Pokud uživatel již přístup k API měl, tak zavolání příkazu mu vygeneruje nový token a starý zneplatní.")]
        public async Task GenerateApiToken(SocketUser userMention)
        {
            using var service = GetService<WebAccessService>();

            var token = await service.Service.CreateApiTokenAsync(Context.Guild, userMention);
            await userMention.SendPrivateMessageAsync($"Byl ti vygenerován nový token pro přístup k REST API.\nTvůj token je: `{token}`");
            await ReplyAsync("Token vygenerován a odeslán do PM");
        }

        [Command("releaseApiToken")]
        [Summary("Uvolnění vygenerovaného tokenu k REST API.")]
        public async Task RelaseApiToken(SocketUser userMention)
        {
            try
            {
                using var service = GetService<WebAccessService>();

                await service.Service.RemoveApiTokenAsync(Context.Guild, userMention);
                await ReplyAsync("Token byl z databáze uvolněn.");
            }
            catch (ValidationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("addToWebAdmin")]
        [Summary("Udělení přístupu uživatele do webové administrace.")]
        public async Task AddUserToWebAdminAsync(IUser userMention)
        {
            try
            {
                if (userMention is not SocketUser user)
                    return;

                if (!userMention.IsUser())
                {
                    await ReplyAsync("Do administrace lze přidat pouze uživatele.");
                    return;
                }

                using var service = GetService<WebAccessService>();
                var password = await service.Service.CreateWebAdminAccessAsync(Context.Guild, user);

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
        public async Task RemoveUserFromWebAdminAsync(IUser userMention)
        {
            try
            {
                if (userMention is not SocketUser user)
                    return;

                using var service = GetService<WebAccessService>();

                await service.Service.RemoveWebAdminAccessAsync(Context.Guild, user);
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
            var user = (userMention as SocketGuildUser) ?? await Context.Guild.GetUserFromGuildAsync(userMention.Id);

            if (user == null)
            {
                await ReplyAsync("Uživatel nebyl na tomto serveru nalezen.");
                return;
            }

            using var service = GetService<UserService>();
            var userDetail = await service.Service.GetUserAsync(Context.Guild, user);

            if (userDetail == null)
            {
                await ReplyAsync("Uživatel nebyl v databázi nalezen. Buď ještě není na tomto serveru, nebo neprojevil aktivitu.");
                return;
            }

            var mostActiveChannel = userDetail.GetMostActiveChannel();
            var lastActiveChannel = userDetail.GetLastActiveChannel();

            var embed = await UserInfoHelper.CreateSimpleEmbedAsync(userDetail, Context);
            embed
                .AddField("Práva", string.Join(", ", userDetail.User.GuildPermissions.GetPermissionsNames()), false);

            if (mostActiveChannel != null)
                embed.AddField("Nejaktivnější kanál", $"<#{mostActiveChannel.Channel.Id}> ({mostActiveChannel.Count.FormatWithSpaces()})", false);

            if (lastActiveChannel != null)
                embed.AddField("Poslední zpráva v", $"<#{lastActiveChannel.Channel.Id}> ({lastActiveChannel.LastMessageAt.ToLocaleDatetime()})", false);

            var detailFlags = userDetail.GetDetailFlags();
            if(detailFlags.Count > 0)
                embed.AddField("Detaily", string.Join(", ", detailFlags), false);

            if (userDetail.WebAdminLoginCount != null)
                embed.AddField("Počet přihlášení", userDetail.WebAdminLoginCount.Value.FormatWithSpaces(), true);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("access")]
        [Summary("Zjištění, kam zadaný uživatel vidí.")]
        public async Task GetAccessListAsync(IUser user)
        {
            var guildUser = user as SocketGuildUser;
            await Context.Guild.SyncGuildAsync();

            var textChannels = Context.Guild.TextChannels
                .Where(o => o.HaveAccess(guildUser))
                .OrderBy(o => o.Position)
                .GroupBy(o => o.Category?.Name ?? "Neznámá kategorie")
                .Select(o => new { Category = o.Key, Channels = o.SplitInParts(30).Select(x => x.Select(t => $"<#{t.Id}>")) });

            var voiceChannels = Context.Guild.VoiceChannels
                .Where(o => o.HaveAccess(guildUser))
                .OrderBy(o => o.Position)
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

        [Command("setBotAdmin")]
        [Summary("Nastavení nejvyššího přístupu k botovi.")]
        public async Task SetBotAdminAsync(IUser user, bool isAdmin)
        {
            var guildUser = (user as SocketGuildUser) ?? await user.ConvertToGuildUserAsync(Context.Guild);

            using var service = GetService<UserService>();
            await service.Service.SetBotAdminAsync(Context.Guild, guildUser, isAdmin);
            await ReplyAsync($"Přístup byl úspěšně {(isAdmin ? "udělen" : "odebrán")}");
        }
    }
}
