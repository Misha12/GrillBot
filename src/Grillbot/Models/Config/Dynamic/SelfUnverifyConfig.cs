using System.Collections.Generic;

namespace Grillbot.Models.Config.Dynamic
{
    public class SelfUnverifyConfig
    {
        public int MaxRolesToKeep { get; set; }
        public Dictionary<string, List<string>> RolesToKeep { get; set; }

        public SelfUnverifyConfig()
        {
            RolesToKeep = new Dictionary<string, List<string>>();
        }
    }
}