using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Extensions.Discord;
using Grillbot.Services.Permissions.Preconditions;
using Grillbot.Services.UserManagement;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("user")]
    [RequirePermissions]
    [Name("Správa uživatelů")]
    public class UserModule : BotModuleBase
    {
        private UserService UserService { get; }

        public UserModule(UserService userService)
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
    }
}
