using Discord.Commands;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using System;
using System.Threading.Tasks;

namespace Grillbot.Repository
{
    public class BirthdaysRepository : RepositoryBase
    {
        public BirthdaysRepository(Configuration config) : base(config)
        {
        }

        public async Task<Birthday> AddBirthdayAsync(bool acceptAge, DateTime date, SocketCommandContext context)
        {
            var entity = new Birthday()
            {
                AcceptAge = acceptAge,
                Date = date.Date,
                ChannelIDSnowflake = context.Message.Channel.Id,
                GuildIDSnowflake = context.Guild.Id,
                IDSnowflake = context.Message.Author.Id
            };

            await Context.Set<Birthday>().AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return entity;
        }
    }
}
