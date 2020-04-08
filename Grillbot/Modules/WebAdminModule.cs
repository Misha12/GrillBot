using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Preconditions;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("webadmin")]
    [Name("Sprava webove administrace")]
    public class WebAdminModule : BotModuleBase
    {
        private WebAuthRepository Repository { get; }

        public WebAdminModule(WebAuthRepository repository)
        {
            Repository = repository;
        }

        [DisabledPM]
        [Command("add")]
        [Summary("Udělení přístupu uživatele do webové administrace.")]
        [Remarks("AllowType znamená typ povolení (Allow=0, Deny=1)\nHeslo je volitelné. Pokud nebude zadáno, tak bude vygenerováno náhodné.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")] // Data is from mention.
        public async Task AddUser(string user, string password = null)
        {
            await DoAsync(async () =>
            {
                var user = GetUserFromMention();
                password = Repository.AddUser(Context.Guild, user, password);

                await user?.SendPrivateMessageAsync(
                    $"Byl ti udělen přístup do webové administrace. Uživatelské jméno je tvůj globální discord nick.\nHeslo máš zde: `{password}`. Uchovej si ho.");
            });
        }

        [DisabledPM]
        [Command("remove")]
        [Summary("Odebrání uživatele z webové administrace.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")] // Data is from mention.
        public async Task RemoveUser(string userID)
        {
            await DoAsync(async () =>
            {
                var user = GetUserFromMention();
                Repository.RemoveUser(Context.Guild, user);

                await ReplyAsync("Přístup byl odebrán.");
            });
        }

        [DisabledPM]
        [Command("reset")]
        [Summary("Reset hesla do administrace.")]
        [Remarks("Heslo je volitelné. Pokud nebude zadáno, tak bude vygenerováno ")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")] // Data is from mention.
        public async Task ResetPassword(string userID, string password = null)
        {
            await DoAsync(async () =>
            {
                var user = GetUserFromMention();
                password = Repository.ResetPassword(Context.Guild, user, password);

                await user?.SendPrivateMessageAsync($"Bylo ti obnoveno heslo.\nNové heslo je `{password}`. Uchovej si ho.");
            });
        }

        private SocketGuildUser GetUserFromMention()
        {
            return Context.Message.MentionedUsers.OfType<SocketGuildUser>().FirstOrDefault();
        }
    }
}
