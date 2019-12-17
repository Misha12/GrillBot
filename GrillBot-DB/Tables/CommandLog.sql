CREATE TABLE [dbo].[CommandLog]
(
    [ID] BIGINT NOT NULL PRIMARY KEY IDENTITY (1, 1),
    [Group] VARCHAR(100) NULL,
    [Command] VARCHAR(100) NOT NULL,
    [UserID] VARCHAR(255) NOT NULL,
    [CalledAt] DATETIME NOT NULL CONSTRAINT [DF_CommandLog_CalledAt] DEFAULT (getdate()),
    [FullCommand] VARCHAR(MAX) NOT NULL,
    [GuildID] VARCHAR(255) NULL,
    [ChannelID] VARCHAR(255) NOT NULL
);
