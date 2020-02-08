using Grillbot.Database.Repository;
using Grillbot.Services.Config.Models;
using System;
using System.Threading.Tasks;

namespace Grillbot.Database
{
    public class GrillBotRepository : IDisposable
    {
        private GrillBotContext Context { get; }

        public GrillBotRepository(Configuration config)
        {
            if (string.IsNullOrEmpty(config.Database))
                throw new ArgumentException("Missing databse connection string in configuration");

            Context = new GrillBotContext(config.Database);

            InitRepositories();
        }

        private void InitRepositories()
        {
            AutoReply = new AutoReplyRepository(Context);
            Birthdays = new BirthdaysRepository(Context);
            BotDb = new BotDbRepository(Context);
            EmoteStats = new EmoteStatsRepository(Context);
            ChannelStats = new ChannelStatsRepository(Context);
            Log = new LogRepository(Context);
            TeamSearch = new TeamSearchRepository(Context);
            TempUnverify = new TempUnverifyRepository(Context);
        }

        public void Dispose()
        {
            Context.Dispose();
        }

        public AutoReplyRepository AutoReply { get; private set; }
        public BirthdaysRepository Birthdays { get; private set; }
        public BotDbRepository BotDb { get; private set; }
        public EmoteStatsRepository EmoteStats { get; private set; }
        public ChannelStatsRepository ChannelStats { get; private set; }
        public LogRepository Log { get; private set; }
        public TeamSearchRepository TeamSearch { get; private set; }
        public TempUnverifyRepository TempUnverify { get; private set; }
    }
}