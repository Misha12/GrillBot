using System.Collections.Generic;

namespace Grillbot.Models.Config.Dynamic
{
    public class MemeImagesConfig
    {
        public List<string> AllowedImageTypes { get; set; }

        public MemeImagesConfig()
        {
            AllowedImageTypes = new List<string>();
        }
    }
}
