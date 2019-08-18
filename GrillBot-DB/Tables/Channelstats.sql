CREATE TABLE [dbo].[Channelstats]
(
	[ID] VARCHAR(32) NOT NULL PRIMARY KEY,
	[Count] BIGINT NOT NULL,
	[LastMessageAt] DATETIME2 NOT NULL CONSTRAINT [DF_Channelstats_LastMessageAt] DEFAULT (Cast('0000-00-00T00:00:00' as datetime2))
)
