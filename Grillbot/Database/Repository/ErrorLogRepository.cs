using Grillbot.Database.Entity;
using System;

namespace Grillbot.Database.Repository
{
    public class ErrorLogRepository : RepositoryBase
    {
        public ErrorLogRepository(GrillBotContext context) : base(context)
        {
        }

        public ErrorLogItem CreateRecord(string message)
        {
            var entity = new ErrorLogItem()
            {
                CreatedAt = DateTime.Now,
                Data = message
            };

            Context.Set<ErrorLogItem>().Add(entity);
            Context.SaveChanges();

            return entity;
        }
    }
}
