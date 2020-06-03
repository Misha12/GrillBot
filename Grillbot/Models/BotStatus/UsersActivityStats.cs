using Discord;
using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Models.BotStatus
{
    public class UsersActivityStats
    {
        public int Online { get; set; }
        public int Idle { get; set; }
        public int DoNotDisturb { get; set; }
        public int Offline { get; set; }

        public static async Task<UsersActivityStats> CreateAsync(DiscordSocketClient client)
        {
            foreach (var guild in client.Guilds)
                await guild.SyncGuildAsync();

            var users = client.Guilds.SelectMany(o => o.Users).DistinctBy(o => o.Id).ToList();

            return new UsersActivityStats()
            {
                DoNotDisturb = users.Count(o => o.Status == UserStatus.DoNotDisturb),
                Idle = users.Count(o => o.Status == UserStatus.Idle || o.Status == UserStatus.AFK),
                Offline = users.Count(o => o.Status == UserStatus.Offline || o.Status == UserStatus.Invisible),
                Online = users.Count(o => o.Status == UserStatus.Online)
            };
        }
    }
}
