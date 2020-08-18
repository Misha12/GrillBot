using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Enums;
using Newtonsoft.Json.Linq;
using System;

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
    }
}
