using Discord.WebSocket;
using Grillbot.Database.Repository;
using Grillbot.Exceptions;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> UpdateUnverifyAsync(int id, string time, SocketUser fromUser)
        {
            var timeParser = Provider.GetService<TempUnverifyTimeParser>();
            var unverifyTime = timeParser.Parse(time, minimumMinutes: 10);
            var item = Data.Find(o => o.ID == id);

            if (item == null)
                throw new BotCommandInfoException($"Reset pro ID {id} nelze provést. Záznam nebyl nalezen");

            var guild = Client.GetGuild(item.GuildIDSnowflake);
            var user = await guild.GetUserFromGuildAsync(item.UserID).ConfigureAwait(false);

            using var logService = Provider.GetService<TempUnverifyLogService>();
            logService.LogUpdate(unverifyTime, fromUser, user, guild);

            using var repository = Provider.GetService<TempUnverifyRepository>();
            await repository.UpdateTimeAsync(id, unverifyTime);

            item.TimeFor = unverifyTime;
            item.StartAt = DateTime.Now;
            item.ReInitTimer(ReturnAccess);

            await user.SendPrivateMessageAsync(GetFormatedPrivateMessage(user, item, null, true)).ConfigureAwait(false);
            return $"Reset času pro záznam o dočasném odebrání přístupu s ID **{id}** byl úspěšně aktualizován. " +
                $"Role budou navráceny **{item.GetEndDatetime().ToLocaleDatetime()}**.";
        }
    }
}
