using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace WatchDog_Bot.Repository
{
    public abstract class RepositoryBase : IDisposable
    {
        private SqlConnection Connection { get; }

        protected RepositoryBase(IConfigurationRoot config)
        {
            var connectionString = config["Database"];

            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Missing database connection string");

            Connection = new SqlConnection(connectionString);
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        protected async Task<T> ExecuteCommand<T>(string sql, Func<SqlDataReader, Task<T>> processData)
        {
            await Connection.OpenAsync();

            try
            {
                using(var command = new SqlCommand(sql, Connection))
                {
                    using(var reader = await command.ExecuteReaderAsync())
                    {
                        return await processData(reader);
                    }
                }
            }
            finally
            {
                Connection.Close();
            }
        }
    }
}
