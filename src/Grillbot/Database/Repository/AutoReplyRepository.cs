using Grillbot.Database.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class AutoReplyRepository : RepositoryBase
    {
        public AutoReplyRepository(GrillBotContext context) : base(context)
        {
        }

        public List<AutoReplyItem> GetAllItems()
        {
            return Context.AutoReply.ToList();
        }

        public async Task SetActiveStatusAsync(int id, bool disabled)
        {
            var item = Context.AutoReply.FirstOrDefault(o => o.ID == id);

            if (item == null)
                return;

            item.IsDisabled = disabled;
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task AddItemAsync(AutoReplyItem item)
        {
            await Context.AutoReply.AddAsync(item).ConfigureAwait(false);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task EditItemAsync(int id, string mustContains, string reply, string compareType, bool caseSensitive)
        {
            var item = Context.AutoReply.FirstOrDefault(o => o.ID == id);

            if (item == null)
                return;

            item.MustContains = mustContains;
            item.ReplyMessage = reply;
            item.CaseSensitive = caseSensitive;
            item.SetCompareType(compareType);

            await Context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task RemoveItemAsync(int id)
        {
            var item = Context.AutoReply.FirstOrDefault(o => o.ID == id);

            if (item == null)
                return;

            Context.AutoReply.Remove(item);
            await Context.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
