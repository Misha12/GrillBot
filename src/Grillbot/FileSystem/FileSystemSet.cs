using Grillbot.FileSystem.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOPath = System.IO.Path;

namespace Grillbot.FileSystem
{
    public class FileSystemSet<T> : IList<T> where T : FileSystemEntity, new()
    {
        private string Path { get; }
        private List<TrackedEntity> ChangeTracker { get; }
        private readonly object _locker = new object();

        public FileSystemSet(string path)
        {
            ChangeTracker = new List<TrackedEntity>();
            Path = path;
        }

        /// <summary>
        /// Finds file from disk.
        /// </summary>
        /// <param name="index"></param>
        /// <remarks>File must exists on disk. New files is ignored.</remarks>
        public T this[int index]
        {
            get {
                var entity = GetFilesList().Skip(index).FirstOrDefault();

                if (entity == null)
                    return null;

                lock(_locker)
                {
                    var tracked = ChangeTracker.Find(o => o.Entity.FullPath == entity.FullPath);
                    if (tracked != null)
                        return tracked.Entity as T;
                }

                SetContentToFile(entity);
                return entity;
            }
            set => throw new NotSupportedException();
        }

        public int Count => GetFilesList().Count();
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (_locker)
            {
                item.Path = Path;
                ChangeTracker.Add(new TrackedEntity(item, EntityState.Added));
            }
        }

        public void Clear()
        {
            lock(_locker)
                ChangeTracker.AddRange(GetFilesList().Select(o => new TrackedEntity(o, EntityState.Deleted)));
        }

        public bool Contains(T item)
        {
            lock(_locker)
                return ChangeTracker.Any(o => o.Entity.FullPath == item.FullPath && o.State == EntityState.Added) || GetFilesList().Any(o => o.FullPath == item.FullPath);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetFilesList().Select(o =>
            {
                SetContentToFile(o);
                return o;
            }).GetEnumerator();
        }

        public IEnumerable<T> GetMetadata()
        {
            return GetFilesList();
        }

        public int IndexOf(T item)
        {
            return GetFilesList().ToList().FindIndex(o => o.FullPath == item.FullPath);
        }

        public void Insert(int index, T item)
        {
            Add(item);
        }

        public void Remove(string filename)
        {
            var entity = GetFilesList().FirstOrDefault(o => o.Filename == filename);
            Remove(entity);
        }

        public bool Remove(T item)
        {
            lock(_locker)
            {
                if (ChangeTracker.Any(o => o.Entity.FullPath == item.FullPath && o.State == EntityState.Deleted))
                    return true;

                if (!Contains(item))
                    return false;

                ChangeTracker.Add(new TrackedEntity(item, EntityState.Deleted));
                return true;
            }
        }

        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task CommitAsync()
        {
            lock(_locker)
            {
                // Create copy of change tracker at this moment. Later items will be ignored.
                foreach (var item in ChangeTracker)
                {
                    switch (item.State)
                    {
                        case EntityState.Added:
                        case EntityState.Modified:
                            File.WriteAllBytes(item.Entity.FullPath, item.Entity.Content);
                            break;
                        case EntityState.Deleted:
                            File.Delete(item.Entity.FullPath);
                            break;
                    }
                }

                ChangeTracker.Clear();
            }
        }

        private IEnumerable<T> GetFilesList()
        {
            var entityType = typeof(T);

            return Directory.GetFiles(Path).OrderBy(o => o).Select(o =>
            {
                var t = new T();

                entityType.GetProperty("Filename").SetValue(t, IOPath.GetFileName(o));
                entityType.GetProperty("Path").SetValue(t, IOPath.GetDirectoryName(o));

                return t;
            });
        }

        private void SetContentToFile(T entity)
        {
            entity.Content = File.ReadAllBytes(entity.FullPath);
        }

        public static FileSystemSet<T> Create(string basePath, string name)
        {
            var finalPath = IOPath.Combine(basePath, name);

            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            return new FileSystemSet<T>(finalPath);
        }
    }
}
