using Grillbot.Database.Entity;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Entity.UnverifyLog;
using Microsoft.EntityFrameworkCore;

namespace Grillbot.Database
{
    public class GrillBotContext : DbContext
    {
        public GrillBotContext(DbContextOptions<GrillBotContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<MethodsConfig>().HasMany(o => o.Permissions).WithOne(o => o.Method);
        }

        public virtual DbSet<TeamSearch> TeamSearch { get; set; }
        public virtual DbSet<ChannelStat> ChannelStats { get; set; }
        public virtual DbSet<EmoteStat> EmoteStats { get; set; }
        public virtual DbSet<AutoReplyItem> AutoReply { get; set; }
        public virtual DbSet<TempUnverifyItem> TempUnverify { get; set; }
        public virtual DbSet<UnverifyLog> UnverifyLog { get; set; }
        public virtual DbSet<Birthday> Birthdays { get; set; }
        public virtual DbSet<MethodsConfig> MethodsConfig { get; set; }
        public virtual DbSet<MethodPerm> MethodPerms { get; set; }
        public virtual DbSet<WebAuthPerm> WebAdminPerms { get; set; }
    }
}
