using Grillbot.Database.Entity;
using Grillbot.Database.Entity.Config;
using Grillbot.Database.Entity.Math;
using Grillbot.Database.Entity.MethodConfig;
using Grillbot.Database.Entity.Unverify;
using Grillbot.Database.Entity.Users;
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

            modelBuilder.Entity<DiscordUser>(builder =>
            {
                builder.HasMany(o => o.Channels).WithOne(o => o.User);
                builder.HasMany(o => o.MathAudit).WithOne(o => o.User);
                builder.HasMany(o => o.Reminders).WithOne(o => o.User);
                builder.HasMany(o => o.CreatedInvites).WithOne(o => o.Creator);
                builder.HasMany(o => o.UsedEmotes).WithOne(o => o.User);
                builder.HasMany(o => o.IncomingUnverifyOperations).WithOne(o => o.ToUser).OnDelete(DeleteBehavior.NoAction);
                builder.HasMany(o => o.OutgoingUnverifyOperations).WithOne(o => o.FromUser).OnDelete(DeleteBehavior.NoAction);
                builder.HasOne(o => o.Birthday).WithOne(o => o.User);
                builder.HasOne(o => o.Statistics).WithOne(o => o.User);
                builder.HasOne(o => o.UsedInvite).WithMany(o => o.UsedUsers);
                builder.HasOne(o => o.Unverify).WithOne(o => o.User);

                builder.HasIndex(o => o.UserID);
                builder.HasIndex(o => o.GuildID);
            });

            modelBuilder.Entity<UserChannel>(builder =>
            {
                builder.HasIndex(o => o.UserID);
            });

            modelBuilder.Entity<Reminder>(builder =>
            {
                builder.HasOne(o => o.FromUser);
            });

            modelBuilder.Entity<EmoteStatItem>(builder =>
            {
                builder.HasKey(o => new { o.EmoteID, o.UserID });
            });

            modelBuilder.Entity<Unverify>(builder =>
            {
                builder.HasOne(o => o.SetLogOperation).WithOne(o => o.Unverify);
            });
        }

        public virtual DbSet<TeamSearch> TeamSearch { get; set; }
        public virtual DbSet<AutoReplyItem> AutoReply { get; set; }
        public virtual DbSet<MethodsConfig> MethodsConfig { get; set; }
        public virtual DbSet<DiscordUser> Users { get; set; }
        public virtual DbSet<UserChannel> UserChannels { get; set; }
        public virtual DbSet<MathAuditLogItem> MathAuditLogs { get; set; }
        public virtual DbSet<GlobalConfigItem> GlobalConfig { get; set; }
        public virtual DbSet<Reminder> Reminders { get; set; }
        public virtual DbSet<Invite> Invites { get; set; }
        public virtual DbSet<EmoteStatItem> EmoteStatistics { get; set; }
        public virtual DbSet<Unverify> Unverifies { get; set; }
        public virtual DbSet<UnverifyLog> UnverifyLogs { get; set; }
        public virtual DbSet<ErrorLogItem> Errors { get; set; }
    }
}
