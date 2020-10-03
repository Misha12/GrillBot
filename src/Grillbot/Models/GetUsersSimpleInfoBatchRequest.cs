using System.Collections.Generic;

namespace Grillbot.Models
{
    public class GetUsersSimpleInfoBatchRequest
    {
        public List<ulong> UserIDs { get; set; }
    }
}
