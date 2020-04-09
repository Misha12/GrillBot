using Discord.WebSocket;
using Grillbot.Extensions;
using Grillbot.Extensions.Discord;
using Grillbot.Database.Entity.UnverifyLog;
using System;
using System.Threading.Tasks;

namespace Grillbot.Services.TempUnverify
{
    public partial class TempUnverifyService
    {
        public async Task<string> UpdateUnverifyAsync(int id, string time, SocketUser fromUser)
        {
            var unverifyTime = ParseUnverifyTime(time);
            var item = Data.Find(o => o.ID == id);

            if (item == null)
                throw new ArgumentException($"Reset pro ID {id} nelze provést. Záznam nebyl nalezen");

            var guild = Client.GetGuild(item.GuildIDSnowflake);
            var user = await guild.GetUserFromGuildAsync(item.UserID).ConfigureAwait(false);

            var logData = new UnverifyLogUpdate() { TimeFor = $"{time} ({unverifyTime})" };
            logData.SetUser(user);

            using var repository = Factories.GetUnverifyRepository();
            await repository.LogOperationAsync(UnverifyLogOperation.Update, fromUser, guild, logData).ConfigureAwait(false);
            await repository.UpdateTimeAsync(id, unverifyTime).ConfigureAwait(false);

            item.TimeFor = unverifyTime;
            item.StartAt = DateTime.Now;
            item.ReInitTimer(ReturnAccess);

            await user.SendPrivateMessageAsync(GetFormatedPrivateMessage(user, item, null, true)).ConfigureAwait(false);
            return $"Reset času pro záznam o dočasném odebrání přístupu s ID **{id}** byl úspěšně aktualizován. " +
                $"Role budou navráceny **{item.GetEndDatetime().ToLocaleDatetime()}**.";
        }
    }
}
