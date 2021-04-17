using Grillbot.FileSystem.Entities;
using Grillbot.FileSystem.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.FileSystem
{
    public interface IFileSystemRepository
    {
        AuditLogsRepository AuditLogs { get; }

        Dictionary<string, List<FileSystemEntity>> GetAllFiles();

        Task CommitAsync();
    }
}
