using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class UnverifyRepository : RepositoryBase
    {
        public UnverifyRepository(GrillBotContext context) : base(context)
        {
        }

        public UnverifyLog SaveLogOperation(UnverifyLogOperation operation, JObject jsonData, long fromUserID, long toUserID)
        {
            var entity = new UnverifyLog()
            {
                CreatedAt = DateTime.Now,
                FromUserID = fromUserID,
                Json = jsonData,
                Operation = operation,
                ToUserID = toUserID
            };

            Context.UnverifyLogs.Add(entity);
            Context.SaveChanges();

            return entity;
        }

        public void RemoveUnverify(ulong guildID, ulong userID)
        {
            var unverify = Context.Unverifies
                .Include(o => o.User)
                .FirstOrDefault(o => o.User.GuildID == guildID.ToString() && o.User.UserID == userID.ToString());

            if (unverify == null)
                return;

            Context.Unverifies.Remove(unverify);
            Context.SaveChanges();
        }

        public Unverify FindUnverifyByID(long id)
        {
            return Context.Unverifies.Include(o => o.User)
                .FirstOrDefault(o => o.UserID == id);
        }
    }
}
