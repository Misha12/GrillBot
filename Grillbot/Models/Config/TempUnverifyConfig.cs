using System.Collections.Generic;

namespace Grillbot.Models.Config
{
    public class TempUnverifyConfig
    {
        public List<string> PreprocessRemoveAccess { get; set; }

        public TempUnverifyConfig()
        {
            PreprocessRemoveAccess = new List<string>();
        }
    }
}
