using Grillbot.Services.Unverify.Models;
using System.Collections.Generic;

namespace Grillbot.Models.TempUnverify.Admin
{
    public class UnverifyCurrentStatusViewModel
    {
        public List<UnverifyUserProfile> Unverifies { get; }

        public UnverifyCurrentStatusViewModel(List<UnverifyUserProfile> unverifies)
        {
            Unverifies = unverifies;
        }
    }
}
