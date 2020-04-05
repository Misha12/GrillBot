using System.Collections.Generic;

namespace Grillbot.Models.Config
{
    public class MemeImagesConfig
    {
        public string Path { get; set; }
        public List<string> AllowedImageTypes { get; set; }

        public MemeImagesConfig()
        {
            AllowedImageTypes = new List<string>();
        }
    }
}
