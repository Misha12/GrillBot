using System.Collections.Generic;

namespace Grillbot.Models.TeamSearch
{
    public class TeamSearchViewModel
    {
        public List<TeamSearchItem> Items { get; set; }

        public TeamSearchViewModel(List<TeamSearchItem> items)
        {
            Items = items;
        }
    }
}
