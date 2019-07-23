using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GrilBot.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(IConfigurationRoot config) : base(config)
        {
        }

        public async Task<List<Tuple<ulong, long>>> GetStatistics()
        {
            return await ExecuteCommand("SELECT * FROM Channelstats", async (reader) =>
            {
                var data = new List<Tuple<ulong, long>>();

                while(await reader.ReadAsync())
                {
                    unchecked
                    {
                        ulong channelID = Convert.ToUInt64(reader["ID"]);
                        long count = Convert.ToInt64(reader["Count"]);

                        data.Add(new Tuple<ulong, long>(channelID, count));
                    }
                }

                return data;
            });
        }

        public async Task SaveStatistics(Dictionary<ulong, long> data)
        {
            var sql = "DELETE FROM Channelstats; INSERT INTO Channelstats ([ID], [Count]) VALUES " + 
                string.Join(", ", data.Select(o => $"({o.Key}, {o.Value})"));

            await ExecuteNonReaderQuery(sql, (_) => { });
        }
    }
}
