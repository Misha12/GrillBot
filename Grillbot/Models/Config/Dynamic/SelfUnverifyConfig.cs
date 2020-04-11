using System.Collections.Generic;

namespace Grillbot.Models.Config.Dynamic
{
    public class SelfUnverifyConfig
    {
        public int MaxSubjectsCount { get; set; }
        public List<string> Subjects { get; set; }

        public SelfUnverifyConfig()
        {
            Subjects = new List<string>();
        }
    }
}