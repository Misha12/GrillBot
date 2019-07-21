using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
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
            Connection.Open();
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        protected async Task<T> ExecuteCommand<T>(string sql, Func<SqlDataReader, Task<T>> processData)
        {
            if(Connection.State != System.Data.ConnectionState.Open)
                await Connection.OpenAsync();

            using(var command = new SqlCommand(sql, Connection))
            {
                using(var reader = await command.ExecuteReaderAsync())
                {
                    return await processData(reader);
                }
            }
        }

        protected string GetOrderType(bool ascending, params string[] columns)
        {
            if (columns == null || columns.Length == 0) return "";
            return $"ORDER BY {string.Join(", ", columns)} {(ascending ? "ASC" : "DESC")}";
        }

        protected async Task ExecuteNonReaderQuery(string sql, Action<int> action)
        {
            if (Connection.State != System.Data.ConnectionState.Open)
                await Connection.OpenAsync();

            using(var command = new SqlCommand(sql, Connection))
            {
                var result = await command.ExecuteNonQueryAsync();
                action(result);
            }
        }
    }
}
