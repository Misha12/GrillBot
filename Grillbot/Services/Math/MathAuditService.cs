using Discord;
using Discord.WebSocket;
using Grillbot.Database.Entity.Math;
using Grillbot.Database.Repository;
using Grillbot.Models.Math;
using Newtonsoft.Json;
using System;

namespace Grillbot.Services.Math
{
    public class MathAuditService : IDisposable
    {
        private UsersRepository UsersRepository { get; }

        public MathAuditService(UsersRepository repository)
        {
            UsersRepository = repository;
        }

        public void SaveItem(SocketGuildUser user, IChannel channel, MathSession session, MathCalcResult result)
        {
            var unitInfo = new MathUnitInfo()
            {
                ComputeLimit = session.ComputingTime,
                ForBooster = session.ForBooster,
                SessionID = session.ID
            };

            var dbUser = UsersRepository.GetOrCreateUser(user.Guild.Id, user.Id, false, false, true);

            dbUser.MathAudit.Add(new MathAuditLogItem()
            {
                ChannelIDSnowflake = channel.Id,
                DateTime = DateTime.Now,
                Expression = session.Expression,
                Result = JsonConvert.SerializeObject(result),
                UnitInfo = JsonConvert.SerializeObject(unitInfo)
            });

            UsersRepository.SaveChanges();
        }

        public void Dispose()
        {
            UsersRepository.Dispose();
        }
    }
}
