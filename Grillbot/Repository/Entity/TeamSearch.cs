using System;
using System.Collections.Generic;

namespace Grillbot.Repository.Entity
{
    public partial class TeamSearch
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ChannelId { get; set; }
        public string MessageId { get; set; }
    }
}
