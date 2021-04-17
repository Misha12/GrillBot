using Grillbot.FileSystem.Entities;
using Grillbot.FileSystem.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.FileSystem
{
    public class FileSystemRepository : IFileSystemRepository
    {
        private FileSystemContext Context { get; }
        private List<RepositoryBase> Repositories { get; }

        public AuditLogsRepository AuditLogs => FindOrCreateRepository<AuditLogsRepository>();

        public FileSystemRepository(FileSystemContext context)
        {
            Context = context;
            Repositories = new List<RepositoryBase>();
        }

        private TRepository FindOrCreateRepository<TRepository>() where TRepository : RepositoryBase
        {
            var repository = Repositories.OfType<TRepository>().FirstOrDefault();

            if (repository == null)
            {
                repository = (TRepository)Activator.CreateInstance(typeof(TRepository), new[] { Context });
                Repositories.Add(repository);
            }

            return repository;
        }

        public Dictionary<string, List<FileSystemEntity>> GetAllFiles()
        {
            return Context.GetAllFiles();
        }

        public Task CommitAsync()
        {
            return Context.CommitAsync();
        }
    }
}
