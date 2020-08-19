using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyCurrentStatusViewModel
    {
        public List<UnverifyInfo> Unverifies { get; }

        public UnverifyCurrentStatusViewModel(List<UnverifyInfo> unverifies)
        {
            Unverifies = unverifies;
        }
    }
}
