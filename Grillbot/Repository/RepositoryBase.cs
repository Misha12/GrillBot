using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
