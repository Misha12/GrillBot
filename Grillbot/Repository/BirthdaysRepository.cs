using Discord.Commands;
using Discord.WebSocket;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Date = date.Date,
                GuildIDSnowflake = context.Guild.Id,
                IDSnowflake = context.Message.Author.Id,
                AcceptAge = acceptAge
            };

            await Context.Set<Birthday>().AddAsync(entity).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);

            return entity;
        }

        public async Task<List<Birthday>> GetBirthdaysForDayAsync(DateTime date, string guildID)
        {
            var result = new List<Birthday>();

            var query = Context.Birthdays.Where(o => o.GuildID == guildID);
            foreach (var birthday in await query.ToListAsync().ConfigureAwait(false))
            {
                if (birthday.Date.Day == date.Day && birthday.Date.Month == date.Month)
                {
                    result.Add(birthday);
                }
            }

            return result;
        }

        public async Task<bool> ExistsUserAsync(SocketUser user, string guildID)
        {
            var userID = user.Id.ToString();
            return await Context.Birthdays.AnyAsync(o => o.GuildID == guildID && o.ID == userID).ConfigureAwait(false);
        }

        public async Task RemoveAsync(SocketUser user, string guildID)
        {
            var userID = user.Id.ToString();
            var entity = await Context.Birthdays.FirstOrDefaultAsync(o => o.GuildID == guildID && o.ID == userID).ConfigureAwait(false);

            if (entity == null) return;

            Context.Set<Birthday>().Remove(entity);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
