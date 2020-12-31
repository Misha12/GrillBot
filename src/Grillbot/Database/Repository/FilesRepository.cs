using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database.Repository
{
    public class FilesRepository : RepositoryBase
    {
        public FilesRepository(GrillBotContext context) : base(context)
        {
        }

        public Task<File> GetFileAsync(string filename)
        {
            return Context.Files.AsQueryable()
                .SingleOrDefaultAsync(o => o.Filename == filename);
        }

        public IQueryable<Tuple<string, int>> GetFilesList(bool ignoreAuditLogs = false)
        {
            var query = Context.Files.AsQueryable();

            if (ignoreAuditLogs)
                query = query.Where(o => o.AuditLogItemId == null);

            return query.Select(o => Tuple.Create(o.Filename, EF.Functions.DataLength(o.Content) ?? 0));
        }

        public IQueryable<string> GetFilenames()
        {
            return Context.Files.AsQueryable()
                .Where(o => o.AuditLogItemId == null)
                .Select(o => o.Filename);
        }
    }
}
