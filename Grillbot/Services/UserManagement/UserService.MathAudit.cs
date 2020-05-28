using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Math;
using Grillbot.Database.Repository;
using Grillbot.Models.Math;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace Grillbot.Services.UserManagement
{
    public partial class UserService
    {
        public void SaveMathAuditItem(string expression, SocketGuildUser user, IChannel channel, MathSession session, MathCalcResult result)
        {
            using var scope = Services.CreateScope();
            using var repository = scope.ServiceProvider.GetService<UsersRepository>();

            var dbUser = repository.GetOrCreateUser(user.Guild.Id, user.Id, false, false, true);

            dbUser.MathAudit.Add(new MathAuditLogItem()
            {
                ChannelIDSnowflake = channel.Id,
                Expression = expression,
                Result = JsonConvert.SerializeObject(result),
                UnitInfo = JsonConvert.SerializeObject(new MathUnitInfo()
                {
                    ComputeLimit = session.ComputingTime,
                    ForBooster = session.ForBooster,
                    SessionID = session.ID
                }),
                DateTime = DateTime.Now
            });

            repository.SaveChanges();
        }
    }
}
