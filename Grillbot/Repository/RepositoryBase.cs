using Microsoft.Extensions.Configuration;
using System;

namespace Grillbot.Repository
{
    public abstract class RepositoryBase : IDisposable
    {
        protected GrillBotContext Context { get; set; }

        protected RepositoryBase(IConfiguration config)
        {
            var connectionString = config["Database"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Missing database connection string");

            Context = new GrillBotContext(connectionString);
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}
