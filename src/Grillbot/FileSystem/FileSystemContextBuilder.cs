using System;

namespace Grillbot.FileSystem
{
    public class FileSystemContextBuilder
    {
        public string BasePath { get; set; }

        public void Use(string basePath)
        {
            BasePath = basePath ?? Environment.CurrentDirectory;
        }
    }
}
