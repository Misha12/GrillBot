using System.Collections.Generic;

namespace Grillbot.Models.BotStatus
{
    public class WebStatisticsViewModel
    {
        public List<WebStatItem> Users { get; set; }

        public WebStatisticsViewModel(List<WebStatItem> users)
        {
            Users = users;
        }
    }
}
