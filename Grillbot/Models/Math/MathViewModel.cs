using System.Collections.Generic;

namespace Grillbot.Models.Math
{
    public class MathViewModel
    {
        public List<MathSession> Sessions { get; set; }

        public MathViewModel(List<MathSession> sessions)
        {
            Sessions = sessions;
        }
    }
}
