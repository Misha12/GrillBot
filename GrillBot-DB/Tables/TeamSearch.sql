CREATE TABLE [dbo].[TeamSearch]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [UserId] VARCHAR(30)  NOT NULL,
    [ChannelId] VARCHAR(30) NOT NULL,
    [MessageId] VARCHAR(30) NOT NULL,
    [GuildId] VARCHAR(30) NOT NULL
)
