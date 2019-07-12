CREATE TABLE [dbo].[UsersAndMessagesCounter]
(
	[UserID] BIGINT NOT NULL,
	[ChannelID] BIGINT NOT NULL,
	[Count] BIGINT NOT NULL,
	PRIMARY KEY ([UserID], [ChannelID])
)
