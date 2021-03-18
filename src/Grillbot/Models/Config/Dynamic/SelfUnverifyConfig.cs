using System.Collections.Generic;

namespace Grillbot.Models.Config.Dynamic
{
    public class SelfUnverifyConfig
    {
        public int MaxRolesToKeep { get; set; }
        public Dictionary<string, List<string>> RolesToKeep { get; set; }
        public List<ulong> DiscouragedUsers { get; set; }

        public SelfUnverifyConfig()
        {
            RolesToKeep = new Dictionary<string, List<string>>();
            DiscouragedUsers = new List<ulong>();
        }
    }
}