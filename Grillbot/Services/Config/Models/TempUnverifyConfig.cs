using System;
using System.Collections.Generic;

namespace Grillbot.Services.Config.Models
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
