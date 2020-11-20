using Grillbot.Database.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        private GrillBotContext Context { get; }
        private List<RepositoryBase> Repositories { get; }

        public AutoReplyRepository AutoReplyRepository => FindOrCreateRepository<AutoReplyRepository>();
        public BotDbRepository BotDbRepository => FindOrCreateRepository<BotDbRepository>();
        public ConfigRepository ConfigRepository => FindOrCreateRepository<ConfigRepository>();
        public EmoteStatsRepository EmoteStatsRepository => FindOrCreateRepository<EmoteStatsRepository>();
        public ErrorLogRepository ErrorLogRepository => FindOrCreateRepository<ErrorLogRepository>();
        public FilesRepository FilesRepository => FindOrCreateRepository<FilesRepository>();
        public GlobalConfigRepository GlobalConfigRepository => FindOrCreateRepository<GlobalConfigRepository>();
        public ChannelStatsRepository ChannelStatsRepository => FindOrCreateRepository<ChannelStatsRepository>();
        public InviteRepository InviteRepository => FindOrCreateRepository<InviteRepository>();
        public ReminderRepository ReminderRepository => FindOrCreateRepository<ReminderRepository>();
        public TeamSearchRepository TeamSearchRepository => FindOrCreateRepository<TeamSearchRepository>();
        public UnverifyRepository UnverifyRepository => FindOrCreateRepository<UnverifyRepository>();
        public UsersRepository UsersRepository => FindOrCreateRepository<UsersRepository>();

        public UnitOfWork(GrillBotContext context)
        {
            Context = context;
            Repositories = new List<RepositoryBase>();
        }

        private TRepository FindOrCreateRepository<TRepository>() where TRepository : RepositoryBase
        {
            var repository = Repositories.OfType<TRepository>().FirstOrDefault();

            if (repository == null)
            {
                repository = (TRepository)Activator.CreateInstance(typeof(TRepository), new[] { Context });
                Repositories.Add(repository);
            }

            return repository;
        }

        public void Add<TEntity>(TEntity entity) where TEntity : class
        {
            Context.Set<TEntity>().Add(entity);
        }

        public Task AddAsync<TEntity>(TEntity entity) where TEntity : class
        {
            return Context.Set<TEntity>().AddAsync(entity).AsTask();
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public Task CommitAsync()
        {
            return Context.SaveChangesAsync();
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void RemoveCollection<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            entities = entities.Where(o => o != null);

            if (!entities.Any())
                return;

            Context.Set<TEntity>().RemoveRange(entities);
        }

        public void Dispose()
        {
            Context.Dispose();
            Repositories.Clear();
        }
    }
}
