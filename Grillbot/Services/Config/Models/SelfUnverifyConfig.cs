using System.Collections.Generic;

namespace Grillbot.Services.Config.Models
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