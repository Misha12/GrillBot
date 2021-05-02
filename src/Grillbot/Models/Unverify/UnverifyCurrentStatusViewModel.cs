using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Models.Unverify
{
    public class UnverifyCurrentStatusViewModel
    {
        public List<UnverifyInfo> Unverifies { get; }

        public int SelfUnverifyCount => Unverifies.Count(o => o.Profile.IsSelfUnverify);
        public int UnverifyCount => Unverifies.Count(o => !o.Profile.IsSelfUnverify);

        public UnverifyCurrentStatusViewModel(List<UnverifyInfo> unverifies)
        {
            Unverifies = unverifies;
        }
    }
}
