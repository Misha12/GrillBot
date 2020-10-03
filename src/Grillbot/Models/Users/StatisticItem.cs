namespace Grillbot.Models.Users
{
    public class StatisticItem
    {
        public int ApiCallsCount { get; set; }
        public int WebAdminLoginCount { get; set; }

        public StatisticItem(Database.Entity.Users.StatisticItem entity)
        {
            ApiCallsCount = entity.ApiCallCount;
            WebAdminLoginCount = entity.WebAdminLoginCount;
        }
    }
}
