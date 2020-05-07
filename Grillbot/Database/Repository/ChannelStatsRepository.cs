using System.Linq;

namespace Grillbot.Database.Repository
{
    public class ChannelStatsRepository : RepositoryBase
    {
        public ChannelStatsRepository(GrillBotContext context) : base(context)
        {
        }

        public void RemoveChannel(ulong channelID)
        {
            var channels = Context.UserChannels.AsQueryable()
                .Where(o => o.ChannelID == channelID.ToString())
                .ToList();

            Context.UserChannels.RemoveRange(channels);
            Context.SaveChanges();
        }
    }
}
