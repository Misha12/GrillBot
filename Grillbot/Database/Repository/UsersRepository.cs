using Grillbot.Database.Entity.Users;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Grillbot.Database.Repository
{
    public class UsersRepository : RepositoryBase
    {
        public UsersRepository(GrillBotContext context) : base(context)
        {
        }

        public List<DiscordUser> GetAllUsers()
        {
            return Context.Users
                .Include(o => o.Channels)
                .ToList();
        }

        public void UpdateDatabase(List<DiscordUser> users)
        {
            foreach (var user in users)
            {
                var entity = Context.Users
                    .Include(o => o.Channels)
                    .FirstOrDefault(o => o.GuildID == user.GuildID && o.UserID == user.UserID);

                if (entity == null)
                {
                    entity = new DiscordUser()
                    {
                        UserID = user.UserID,
                        GuildID = user.GuildID,
                        GivenReactionsCount = user.GivenReactionsCount,
                        Points = user.Points,
                        ObtainedReactionsCount = user.ObtainedReactionsCount,
                        WebAdminPassword = user.WebAdminPassword,
                        Channels = user.Channels.ToHashSet()
                    };

                    Context.Users.Add(entity);
                }
                else
                {
                    entity.GivenReactionsCount = user.GivenReactionsCount;
                    entity.Points = user.Points;
                    entity.ObtainedReactionsCount = user.ObtainedReactionsCount;
                    entity.WebAdminPassword = user.WebAdminPassword;

                    foreach (var channel in user.Channels)
                    {
                        var channelEntity = entity.Channels.FirstOrDefault(o => o.ChannelID == channel.ChannelID && o.DiscordUserID == channel.DiscordUserID);

                        if (channelEntity == null)
                        {
                            channelEntity = new UserChannel()
                            {
                                Count = channel.Count,
                                LastMessageAt = channel.LastMessageAt,
                                ChannelID = channel.ChannelID,
                                DiscordUserID = channel.DiscordUserID
                            };

                            entity.Channels.Add(channelEntity);
                        }
                        else
                        {
                            channelEntity.Count = channel.Count;
                            channelEntity.LastMessageAt = channel.LastMessageAt;
                        }
                    }
                }
            }

            Context.SaveChanges();
        }
    }
}
