using System.Collections.Generic;

namespace Grillbot.Models.Config.Dynamic
{
    public class TempUnverifyConfig
    {
        public List<string> PreprocessRemoveAccess { get; set; }
        public ulong MutedRoleID { get; set; }

        public TempUnverifyConfig()
        {
            PreprocessRemoveAccess = new List<string>();
        }
    }
}
