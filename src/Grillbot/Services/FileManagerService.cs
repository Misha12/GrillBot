using Grillbot.Database;
using Grillbot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services
{
    public class FileManagerService
    {
        private IGrillBotRepository Repository { get; }

        public FileManagerService(IGrillBotRepository repository)
        {
            Repository = repository;
        }

        public async Task<Dictionary<string, int>> GetFilesAsync(bool ignoreAuditLogs = false)
        {
            var query = Repository.FilesRepository.GetFilesList(ignoreAuditLogs);
            return (await query.ToListAsync()).ToDictionary(o => o.Item1, o => o.Item2);
        }

        public async Task<File> GetFileAsync(string filename)
        {
            return await Repository.FilesRepository.GetFileAsync(filename);
        }

        public async Task DeleteFileAsync(string filename)
        {
            var entity = await Repository.FilesRepository.GetFileAsync(filename);

            if (entity == null)
                return;

            Repository.Remove(entity);
            await Repository.CommitAsync();
        }
    }
}
