using Discord.WebSocket;
using Grillbot.Database;
using Grillbot.Database.Entity.Users;
using Grillbot.Database.Enums.Includes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grillbot.Services.UserManagement
{
    public class UserMessagesService
    {
        private IGrillBotRepository GrillBotRepository { get; }

        public UserMessagesService(IGrillBotRepository grillBotRepository)
        {
            GrillBotRepository = grillBotRepository;
        }

        public async Task IncrementMessageStats(SocketGuild guild, SocketUser user, ISocketMessageChannel channel)
        {
            var userEntity = await GrillBotRepository.UsersRepository.GetOrCreateUserAsync(guild.Id, user.Id, UsersIncludes.Channels);
            var channelEntity = userEntity.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channel.Id);

            if (channelEntity == null)
            {
                channelEntity = new UserChannel()
                {
                    ChannelIDSnowflake = channel.Id,
                    Count = 1,
                    LastMessageAt = DateTime.Now,
                };

                userEntity.Channels.Add(channelEntity);
            }
            else
            {
                channelEntity.Count++;
                channelEntity.LastMessageAt = DateTime.Now;
            }

            await GrillBotRepository.CommitAsync();
        }

        public async Task DecrementMessageStats(SocketGuild guild, SocketUser user, ISocketMessageChannel channel)
        {
            var entity = await GrillBotRepository.UsersRepository.GetUserAsync(guild.Id, user.Id, UsersIncludes.Channels);

            if (entity == null)
                return;

            var channelEntity = entity.Channels.FirstOrDefault(o => o.ChannelIDSnowflake == channel.Id);
            if (channelEntity == null || channelEntity.Count == 0)
                return;

            channelEntity.Count--;
            await GrillBotRepository.CommitAsync();
        }
    }
}
