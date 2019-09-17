using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Repository
{
    public class AutoReplyRepository : RepositoryBase
    {
        public AutoReplyRepository(Configuration config) : base(config)
        {
        }

        public List<AutoReplyItem> GetAllItems()
        {
            return Context.AutoReply.ToList();
        }

        public async Task SetActiveStatus(int id, bool disabled)
        {
            var item = await Context.AutoReply.FirstOrDefaultAsync(o => o.ID == id);

            if (item == null)
                return;

            item.IsDisabled = disabled;
            await Context.SaveChangesAsync();
        }

        public async Task AddItemAsync(AutoReplyItem item)
        {
            await Context.AutoReply.AddAsync(item);
            await Context.SaveChangesAsync();
        }

        public async Task EditItemAsync(int id, string mustContains, string reply)
        {
            var item = await Context.AutoReply.FirstOrDefaultAsync(o => o.ID == id);

            if (item == null)
                return;

            item.MustContains = mustContains;
            item.ReplyMessage = reply;

            await Context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int id)
        {
            var item = await Context.AutoReply.FirstOrDefaultAsync(o => o.ID == id);

            if (item == null)
                return;

            Context.AutoReply.Remove(item);
            await Context.SaveChangesAsync();
        }
    }
}
