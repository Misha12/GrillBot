using System.Collections.Generic;
using System;
using Discord;
using System.Threading.Tasks;
using System.Linq;
using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Views;
using Microsoft.EntityFrameworkCore;

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
            var summarizedData = Context.CommandLog
                .GroupBy(o => new { o.Group, o.Command })
                .OrderByDescending(o => o.Count())
                .Select(o => new SummarizedCommandLog()
                {
                    Command = o.Key.Command ?? "",
                    Count = o.Count(),
                    Group = o.Key.Group ?? ""
                })
                .ToList();

            var groups = summarizedData.Select(o => o.Group).ToList();
            var commands = summarizedData.Select(o => o.Command).ToList();

            var configData = Context.MethodsConfig
                .Include(o => o.Permissions)
                .Where(e => groups.Contains(e.Group) && commands.Contains(e.Command))
                .Select(o => new { id = o.ID, group = o.Group, command = o.Command, guild = o.GuildID, permissionsCount = o.Permissions.Count() })
                .ToList();

            foreach (var item in summarizedData)
            {
                var configItem = configData.Where(o => o.group == item.Group && o.command == item.Command).ToList();

                item.Methods = configItem.ToDictionary(o => o.id, o => o.guild);
                item.PermissionsCount = configItem.Sum(o => o.permissionsCount);
            }

            return summarizedData;
        }
    }
}