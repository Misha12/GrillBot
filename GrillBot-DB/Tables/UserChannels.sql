﻿CREATE TABLE [dbo].[UserChannels]
(
	[ID] BIGINT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[ChannelID] VARCHAR(30) NOT NULL,
	[UserID] BIGINT NOT NULL,
	[Count] BIGINT NOT NULL CONSTRAINT DF_UserChannels_Count DEFAULT (0),
	[LastMessageAt] DATETIME NULL,
	[DiscordUserID] VARCHAR(30) NOT NULL,
	[GuildID] VARCHAR(30) NOT NULL

	CONSTRAINT FK_UserChannels_UserID FOREIGN KEY ([UserID]) REFERENCES [DiscordUsers]([ID])
)

GO
CREATE INDEX IX_UserChannels_UserID ON [dbo].[UserChannels] ([UserID])

GO
CREATE INDEX IX_UserChannels_DiscordUserID ON [dbo].[UserChannels] ([DiscordUserID])