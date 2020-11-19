using System;
using System.Threading.Tasks;

namespace Grillbot.Database
{
    public abstract class RepositoryBase : IDisposable
    {
        protected GrillBotContext Context { get; set; }

        protected RepositoryBase(GrillBotContext context)
        {
            Context = context;
        }

        public void Dispose()
        {
            Context?.Dispose();
        }

        public int SaveChanges()
        {
            return Context.SaveChanges();
        }

        public int SaveChangesIfAny()
        {
            if (!Context.ChangeTracker.HasChanges())
                return 0;

            return Context.SaveChanges();
        }

        public Task<int> SaveChangesAsync()
        {
            return Context.SaveChangesAsync();
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            Context.Set<TEntity>().Remove(entity);
        }
    }
}
