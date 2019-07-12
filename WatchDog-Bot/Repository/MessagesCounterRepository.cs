using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WatchDog_Bot.Repository
{
    public class MessagesCounterRepository : RepositoryBase
    {
        public MessagesCounterRepository(IConfigurationRoot config) : base(config)
        {
        }

        public async Task<List<Tuple<ulong, ulong>>> GetUsersMessageCountAsync(int topCount, bool asceding)
        {
            var sql = $"SELECT TOP({topCount}) UserID, Sum([Count]) AS Count FROM [UsersAndMessagesCounter] GROUP BY UserID ORDER BY Sum([Count]) {(asceding ? "ASC" : "DESC")}";

            return await ExecuteCommand(sql, async (reader) =>
            {
                var data = new List<Tuple<ulong, ulong>>();

                while(await reader.ReadAsync())
                {
                    var db_userID = Convert.ToUInt64(reader["UserID"]);
                    var db_count = Convert.ToUInt64(reader["Count"]);

                    data.Add(new Tuple<ulong, ulong>(db_userID, db_count));
                }

                return data;
            });
        }

        public async Task<List<Tuple<ulong, ulong, ulong>>> GetMessageCounters(ulong? userID, ulong? channelID)
        {
            var crieria = (new[]
            {
                userID == null ? null : $"UserID={userID}",
                channelID == null ? null : $"ChannelID={channelID}"
            }).Where(o => o != null).ToArray();

            var sql = $"SELECT * FROM [UsersAndMessagesCounter] {(crieria.Length > 0 ? "WHERE" + string.Join(" AND ", crieria) : "")}";
            return await ExecuteCommand(sql, async (reader) =>
            {
                var data = new List<Tuple<ulong, ulong, ulong>>();

                while(await reader.ReadAsync())
                {
                    var db_userID = Convert.ToUInt64(reader["UserID"]);
                    var db_channelID = Convert.ToUInt64(reader["ChannelID"]);
                    var db_count = Convert.ToUInt64(reader["Count"]);

                    data.Add(new Tuple<ulong, ulong, ulong>(db_userID, db_channelID, db_count));
                }

                return data;
            });
        }
    }
}
