using System.Collections.Generic;
using System;
using Discord;
using System.Threading.Tasks;
using System.Linq;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Views;

namespace Grillbot.Database.Repository
{
    public class LogRepository : RepositoryBase
    {
        public LogRepository(GrillBotContext context) : base(context)
        {
        }

        public async Task InsertItem(string group, string command, IUser user, DateTime calledAt, string fullCommand, IGuild guild, IChannel channel)
        {
            var item = new CommandLog()
            {
                Group = group,
                Command = command,
                UserIDSnowflake = user.Id,
                CalledAt = calledAt,
                GuildIDSnowflake = guild?.Id,
                ChannelIDSnowflake = channel.Id,
                FullCommand = fullCommand
            };

            await Context.CommandLog.AddAsync(item).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public List<SummarizedCommandLog> GetSummarizedCommandLog()
        {
            return Context.CommandLog
                .GroupBy(o => new { o.Group, o.Command })
                .OrderByDescending(o => o.Count())
                .Select(o => new SummarizedCommandLog()
                {
                    Command = o.Key.Command,
                    Count = o.Count(),
                    Group = o.Key.Group
                })
                .ToList();
        }
    }
}