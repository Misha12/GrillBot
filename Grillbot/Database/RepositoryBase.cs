using System;

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
    }
}
