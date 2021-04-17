using Grillbot.FileSystem.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.FileSystem
{
    public class FileSystemContext
    {
        public string BasePath { get; }
        public FileSystemSet<AuditLogFile> AuditLogs { get; set; }

        public FileSystemContext(FileSystemContextBuilder builder)
        {
            BasePath = builder.BasePath;

            AuditLogs = FileSystemSet<AuditLogFile>.Create(BasePath, nameof(AuditLogs));
        }

        public Dictionary<string, List<FileSystemEntity>> GetAllFiles()
        {
            return new Dictionary<string, List<FileSystemEntity>>()
            {
                { nameof(AuditLogs), AuditLogs.GetMetadata().Select(o => o as FileSystemEntity).ToList() }
            };
        }

        public async Task CommitAsync()
        {
            await AuditLogs.CommitAsync();
        }
    }
}
