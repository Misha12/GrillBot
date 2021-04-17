using Microsoft.EntityFrameworkCore;

namespace Grillbot.FileSystem.Entities
{
    public class TrackedEntity
    {
        public FileSystemEntity Entity { get; set; }
        public EntityState State { get; set; }

        public TrackedEntity(FileSystemEntity entity, EntityState state = EntityState.Added)
        {
            Entity = entity;
            State = state;
        }
    }
}
