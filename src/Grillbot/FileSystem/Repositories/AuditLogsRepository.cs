using Grillbot.FileSystem.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.FileSystem.Repositories
{
    public class AuditLogsRepository : RepositoryBase
    {
        public AuditLogsRepository(FileSystemContext context) : base(context)
        {
        }

        public AuditLogFile GetFileByFilename(string filename)
        {
            return Context.AuditLogs.FirstOrDefault(o => o.Filename == filename);
        }

        public void Add(AuditLogFile file)
        {
            Context.AuditLogs.Add(file);
        }

        public void Remove(AuditLogFile file)
        {
            Context.AuditLogs.Remove(file);
        }

        public void RemoveFile(string filename)
        {
            Context.AuditLogs.Remove(filename);
        }

        public void RemoveFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
                Context.AuditLogs.Remove(file);
        }
    }
}
