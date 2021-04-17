using Grillbot.FileSystem.Entities;

namespace Grillbot.FileSystem.Repositories
{
    public abstract class RepositoryBase
    {
        public FileSystemContext Context { get; }

        protected RepositoryBase(FileSystemContext context)
        {
            Context = context;
        }
    }
}
