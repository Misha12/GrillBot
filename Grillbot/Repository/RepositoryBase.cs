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

        private SqlConnection Connection { get; }

        protected RepositoryBase(IConfiguration config)
        {
            var connectionString = config["Database"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Missing database connection string");

            Context = new GrillBotContext(connectionString);

            Connection = new SqlConnection(connectionString);
            Connection.Open();
        }

        public void Dispose()
        {
            Connection.Dispose();
            Context.Dispose();
        }

        protected async Task<T> ExecuteCommand<T>(string sql, Func<SqlDataReader, Task<T>> processData, params SqlParameter[] parameters)
        {
            if(Connection.State != System.Data.ConnectionState.Open)
                await Connection.OpenAsync();

            using(var command = new SqlCommand(sql, Connection))
            {
                if(parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                using(var reader = await command.ExecuteReaderAsync())
                {
                    return await processData(reader);
                }
            }
        }

        protected async Task ExecuteNonReaderBatch(List<SqlCommand> commands)
        {
            if (Connection.State != ConnectionState.Open)
                await Connection.OpenAsync();

            foreach(var command in commands)
            {
                command.Connection = Connection;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
