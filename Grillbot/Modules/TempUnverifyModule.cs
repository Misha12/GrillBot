using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Group("unverify")]
    [Name("Odebrání přístupu.")]
    public class TempUnverifyModule : BotModuleBase
    {
        //TODO Permissions

        private TempUnverifyService Service { get; }

        public TempUnverifyModule(TempUnverifyService service)
        {
            Service = service;
        }

        [Command("")]
        public async Task SetUnverify(string user, string time)
        {
            await DoAsync(async () =>
            {
                var userToUnverify = Context.Message.MentionedUsers.FirstOrDefault();

                if(userToUnverify is SocketGuildUser socketGuildUser) 
                {
                    var message = await Service.SetUnverify(socketGuildUser, time);
                    await ReplyAsync(message);
                }
            });
        }
    }
}
