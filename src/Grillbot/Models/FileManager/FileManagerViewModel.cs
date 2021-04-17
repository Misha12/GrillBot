using Grillbot.FileSystem.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.FileManager
{
    public class FileManagerViewModel
    {
        public Dictionary<string, List<FileSystemEntity>> Files { get; set; }

        public FileManagerViewModel(Dictionary<string, List<FileSystemEntity>> files)
        {
            Files = files;
        }

        public long TotalSize => Files.Sum(o => o.Value.Sum(x => x.FileInfo.Length));
        public long TotalCount => Files.Sum(o => o.Value.Count);
    }
}
