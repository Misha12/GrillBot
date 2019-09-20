using Discord.WebSocket;
using Grillbot.Repository.Entity;
using Grillbot.Services.Config.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Repository
{
    public class TempUnverifyRepository : RepositoryBase
    {
        public TempUnverifyRepository(Configuration config) : base(config)
        {
        }

        public IQueryable<TempUnverifyItem> GetAllItems()
        {
            return Context.TempUnverify.AsQueryable();
        }

        public async Task<TempUnverifyItem> AddItemAsync(List<string> roles, ulong userID, long timeFor)
        {
            var entity = new TempUnverifyItem()
            {
                DeserializedRolesToReturn = roles,
                TimeFor = timeFor,
                UserID = userID.ToString()
            };

            await Context.TempUnverify.AddAsync(entity);
            await Context.SaveChangesAsync();

            return entity;
        }
    }
}
