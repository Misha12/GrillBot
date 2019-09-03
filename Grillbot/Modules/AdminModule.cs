using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Modules
{
    [Name("Administrační funkce")]
    public class AdminModule : BotModuleBase
    {
        private IConfiguration Config { get; }

        public AdminModule(IConfiguration config)
        {
            Config = config;
        }

        [Command("botadminlist")]
        [Summary("Seznam administrátorů bota.")]
        public async Task GetBotAdminListAsync()
        {
            var botAdmins = Config.GetSection("Discord:Administrators")
                .GetChildren()
                .Select(o => Context.Guild.GetUser(Convert.ToUInt64(o.Value)))
                .ToList();

            var formatedBotAdmins = string.Join(Environment.NewLine, botAdmins.Select(o => $"{o.Username}#{o.Discriminator} ({o.Id})"));
            await ReplyAsync("Seznam administrátorů:" + Environment.NewLine + formatedBotAdmins);
        }
    }
}
