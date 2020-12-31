using System.Collections.Generic;

namespace Grillbot.Models.FileManager
{
    public class FileManagerViewModel
    {
        public Dictionary<string, int> Files { get; set; }

        public FileManagerViewModel(Dictionary<string, int> files)
        {
            Files = files;
        }
    }
}
