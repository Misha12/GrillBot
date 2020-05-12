using System;
using System.Threading.Tasks;

namespace Grillbot.Database
{
    public abstract class RepositoryBase : IDisposable
    {
        protected GrillBotContext Context { get; set; }

        protected RepositoryBase(GrillBotContext context)
        {
            Context = context;
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }
    }
}
