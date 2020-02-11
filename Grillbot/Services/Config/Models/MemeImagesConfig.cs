using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.Config.Models
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
