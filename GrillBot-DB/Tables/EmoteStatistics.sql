CREATE TABLE [dbo].[EmoteStatistics]
(
	[EmoteID] VARCHAR(255) NOT NULL PRIMARY KEY,
	[Count] BIGINT NOT NULL,
	[LastOccuredAt] DATETIME2 NOT NULL CONSTRAINT [DF_EmoteStatistics_LastMessageAt] DEFAULT (Cast('0000-00-00T00:00:00' as datetime2))
)
