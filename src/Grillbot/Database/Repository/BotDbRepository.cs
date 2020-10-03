using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database.Repository
{
    public class BotDbRepository : RepositoryBase
    {
        public BotDbRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task<Dictionary<string, Tuple<int, long>>> GetTableRowsCount()
        {
            // MSSQL Only. Check another command for another SQL.
            // This SQL returns all tables in DB and count of records.

            var counters = new Dictionary<string, Tuple<int, long>>();
            var query = @"SELECT t.NAME AS TableName, p.rows, SUM(a.total_pages) * 8 AS TotalSpaceKB
                          FROM sys.tables t
                          INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id
                          INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id
                          INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
                          LEFT OUTER JOIN sys.schemas s ON t.schema_id = s.schema_id
                          WHERE t.NAME NOT LIKE 'dt%'  AND t.is_ms_shipped = 0 AND i.OBJECT_ID > 255 
                          GROUP BY t.Name, p.Rows ORDER BY TotalSpaceKB DESC, p.rows DESC, t.Name";

            using var command = Context.Database.GetDbConnection().CreateCommand();

            try
            {
                command.CommandText = query;
                Context.Database.OpenConnection();

                using var result = await command.ExecuteReaderAsync();

                while(result.Read())
                {
                    var tableName = result["TableName"].ToString();
                    var count = Convert.ToInt32(result["rows"]);
                    var spaceKb = Convert.ToInt64(result["TotalSpaceKB"]);

                    if (!counters.ContainsKey(tableName))
                        counters.Add(tableName, new Tuple<int, long>(count, spaceKb * 1024));
                }
            }
            finally
            {
                Context.Database.CloseConnection();
            }

            return counters;
        }
    }
}
