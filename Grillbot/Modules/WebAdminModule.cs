using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("webadmin")]
    [RequirePermissions]
    [Name("Správa webové administrace")]
    public class WebAdminModule : BotModuleBase
    {
        private UserService UserService { get; }

        public WebAdminModule(UserService userService)
        {
            UserService = userService;
        }

        [Command("add")]
        [Summary("Udělení přístupu uživatele do webové administrace.")]
        [Remarks("AllowType znamená typ povolení (Allow=0, Deny=1)\nHeslo je volitelné. Pokud nebude zadáno, tak bude vygenerováno náhodné.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")] // Data is from mention.
        public async Task AddUser(string userMention, string password = null)
        {
            try
            {
                var user = GetUserFromMention();

                if (user == null)
                {
                    await ReplyAsync("Nebyl tagnut žádný uživatel.");
                    return;
                }

                password = UserService.AddUserToWebAdmin(Context.Guild, user, password);

                await user?.SendPrivateMessageAsync(
                    $"Byl ti udělen přístup do webové administrace. Uživatelské jméno je tvůj globální discord nick.\nHeslo máš zde: `{password}`. Uchovej si ho.");
                await ReplyAsync("Přístup umožněn.");
            }
            catch (InvalidOperationException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        [Command("remove")]
        [Summary("Odebrání uživatele z webové administrace.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")] // Data is from mention.
        public async Task RemoveUser(string userMention)
        {
            try
            {
                var user = GetUserFromMention();

                if (user == null)
                {
                    await ReplyAsync("Nebyl tagnut žádný uživatel.");
                    return;
                }

                UserService.RemoveUserFromWebAdmin(Context.Guild, user);
                await ReplyAsync("Přístup byl odebrán.");
            }
            catch (ArgumentException ex)
            {
                await ReplyAsync(ex.Message);
            }
        }

        private SocketGuildUser GetUserFromMention()
        {
            return Context.Message.MentionedUsers.OfType<SocketGuildUser>().FirstOrDefault();
        }
    }
}
