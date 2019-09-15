using Grillbot.Services.Config.Models;
using System;

namespace Grillbot.Repository
{
    public abstract class RepositoryBase : IDisposable
    {
        protected GrillBotContext Context { get; set; }

        protected RepositoryBase(Configuration config)
        {
            if (string.IsNullOrEmpty(config.Database))
                throw new ArgumentException("Missing database connection string");

            Context = new GrillBotContext(config.Database);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
