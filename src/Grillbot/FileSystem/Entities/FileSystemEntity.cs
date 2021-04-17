using System.IO;

namespace Grillbot.FileSystem.Entities
{
    public class FileSystemEntity
    {
        public string Filename { get; set; }
        public string Path { get; set; }
        public byte[] Content { get; set; }

        public string FullPath => System.IO.Path.Combine(Path, Filename);
        public FileInfo FileInfo => new FileInfo(FullPath);
    }
}
