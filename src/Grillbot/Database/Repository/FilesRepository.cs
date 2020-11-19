using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
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

        public IQueryable<string> GetFilenames()
        {
            return Context.Files.AsQueryable()
                .Select(o => o.Filename);
        }

        public async Task UploadFileAsync(string filename, byte[] content)
        {
            var entity = await GetFileAsync(filename);

            if (entity == null)
            {
                entity = new File()
                {
                    Content = content,
                    Filename = filename
                };

                await Context.Files.AddAsync(entity);
            }
            else
            {
                entity.Content = content;
            }

            await SaveChangesAsync();
        }
    }
}
