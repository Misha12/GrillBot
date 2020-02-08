using System.Collections.Generic;
using System;
using Discord;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Grillbot.Database.Entity;

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

        public async Task<List<CommandLog>> GetCommandLogsAsync(int topCount)
        {
            return await Context.CommandLog
                .OrderByDescending(o => o.ID)
                .Take(topCount)
                .ToListAsync().ConfigureAwait(false);
        }

        public async Task<CommandLog> GetCommandLogDetailAsync(long id)
        {
            return await Context.CommandLog.FirstOrDefaultAsync(o => o.ID == id).ConfigureAwait(false);
        }
    }
}