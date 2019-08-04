using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Grillbot.Helpers;
using Microsoft.Extensions.Configuration;

namespace Grillbot.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(IConfiguration config) : base(config)
        {
        }

        public async Task<List<Tuple<ulong, long, DateTime>>> GetStatistics()
        {
            return await ExecuteCommand("SELECT * FROM Channelstats", async (reader) =>
            {
                var data = new List<Tuple<ulong, long, DateTime>>();

                while(await reader.ReadAsync())
                {
                    unchecked
                    {
                        ulong channelID = Convert.ToUInt64(reader["ID"]);
                        long count = Convert.ToInt64(reader["Count"]);
                        DateTime lastMessageAt = Convert.ToDateTime(reader["LastMessageAt"]);

                        data.Add(new Tuple<ulong, long, DateTime>(channelID, count, lastMessageAt));
                    }
                }

                return data;
            });
        }

        public async Task UpdateStatistics(Dictionary<ulong, long> dataToUpdate, Dictionary<ulong, DateTime> lastMessageDates)
        {
            var commandBuilder = new List<SqlCommand>();

            try
            {
                var channels = dataToUpdate.Select(o => o.Key.ToString());
                var deleteCommand = new SqlCommand(SqlHelper.BuildWhereInClause("DELETE FROM Channelstats WHERE ID IN ({0})", "ChannelID", channels));
                deleteCommand.AddParamsToCommand("ChannelID", channels);
                commandBuilder.Add(deleteCommand);

                commandBuilder.AddRange(dataToUpdate.Select(o =>
                {
                    var cmd = new SqlCommand("INSERT INTO Channelstats (ID, [Count], [LastMessageAt]) VALUES (@channelID, @count, @lastMessageAt)");

                    cmd.Parameters.AddWithValue("@channelID", o.Key.ToString());
                    cmd.Parameters.AddWithValue("@count", o.Value);
                    cmd.Parameters.AddWithValue("@lastMessageAt", lastMessageDates[o.Key]);

                    return cmd;
                }));

                await ExecuteNonReaderBatch(commandBuilder);
            }
            finally
            {
                foreach (var command in commandBuilder)
                    command.Dispose();
            }
        }
    }
}
