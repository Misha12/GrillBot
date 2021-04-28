using Grillbot.Database.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grillbot.Database
{
    public interface IGrillBotRepository : IDisposable
    {
        AutoReplyRepository AutoReplyRepository { get; }
        BotDbRepository BotDbRepository { get; }
        ConfigRepository ConfigRepository { get; }
        EmoteStatsRepository EmoteStatsRepository { get; }
        ErrorLogRepository ErrorLogRepository { get; }
        FilesRepository FilesRepository { get; }
        ChannelStatsRepository ChannelStatsRepository { get; }
        InviteRepository InviteRepository { get; }
        ReminderRepository ReminderRepository { get; }
        TeamSearchRepository TeamSearchRepository { get; }
        UnverifyRepository UnverifyRepository { get; }
        UsersRepository UsersRepository { get; }
        AuditLogsRepository AuditLogs { get; }

        void Add<TEntity>(TEntity entity) where TEntity : class;
        Task AddAsync<TEntity>(TEntity entity) where TEntity : class;
        void Remove<TEntity>(TEntity entity) where TEntity : class;
        void RemoveCollection<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void Commit();
        Task CommitAsync();
        void Detach<TEntity>(TEntity entity) where TEntity : class;
    }
}
