DROP TABLE IF EXISTS dbo.EmoteStatistics;
CREATE TABLE [dbo].[EmoteStatistics]
(
	[EmoteID] NVARCHAR(255) NOT NULL PRIMARY KEY,
	[Count] BIGINT NOT NULL,
	[LastOccuredAt] DATETIME2 NOT NULL CONSTRAINT [DF_EmoteStatistics_LastMessageAt] DEFAULT (Cast('0000-00-00T00:00:00' as datetime2))
);

INSERT INTO EmoteStatistics SELECT * FROM Grillbot.dbo.EmoteStatistics;

DROP TABLE IF EXISTS dbo.ChannelStats;
CREATE TABLE [dbo].[Channelstats]
(
	[ID] NVARCHAR(32) NOT NULL PRIMARY KEY,
	[Count] BIGINT NOT NULL,
	[LastMessageAt] DATETIME2 NOT NULL CONSTRAINT [DF_Channelstats_LastMessageAt] DEFAULT (Cast('0000-00-00T00:00:00' as datetime2))
);

INSERT INTO Channelstats SELECT * FROM Grillbot.dbo.Channelstats;

DROP TABLE IF EXISTS dbo.TeamSearch;
CREATE TABLE [dbo].[TeamSearch]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
    [UserId] NVARCHAR(255)  NOT NULL,
    [ChannelId] NVARCHAR(255) NOT NULL,
    [MessageId] NVARCHAR(255) NOT NULL
);

INSERT INTO TeamSearch (UserId, ChannelId, MessageId) SELECT UserId, ChannelId, MessageId FROM Grillbot.dbo.TeamSearch;

DROP TABLE IF EXISTS dbo.TempUnverify;
CREATE TABLE [dbo].[TempUnverify]
(
	[ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[GuildID] NVARCHAR(30) NOT NULL,
	[UserID] NVARCHAR(30) NOT NULL,
	[TimeFor] BIGINT NOT NULL,
	[StartAt] DATETIME NOT NULL,
	[RolesToReturn] NVARCHAR(MAX) NOT NULL
);

INSERT INTO TempUnverify (GuildID, UserID, TimeFor, StartAt, RolesToReturn)
	SELECT GuildID, UserID, TimeFor, StartAt, RolesToReturn FROM Grillbot.dbo.TempUnverify;

DROP TABLE IF EXISTS dbo.AutoReply;
CREATE TABLE [dbo].[AutoReply]
(
	[ID] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
	[MustContains] NVARCHAR(MAX) NOT NULL,
	[ReplyMessage] NVARCHAR(MAX) NOT NULL,
	[IsDisabled] BIT NOT NULL,
	[CompareType] INT NOT NULL CONSTRAINT [DF_AutoReply_CompareType] DEFAULT (0),
	[CaseSensitive] BIT NOT NULL CONSTRAINT [DF_AutoReply_CaseSensitive] DEFAULT (0)
);

INSERT INTO AutoReply (MustContains, ReplyMessage, IsDisabled) VALUES 
	('uh oh', 'uh oh', 0),
	('PR', 'https://github.com/Misha12/GrillBot/pulls', 1),
	('Je čerstvá!', 'Není čerstvá', 0),
	('BIA', 'BIA a BIB je čistě doporučené rozdělení, abyste se vlezli do prednáškovky. S klidným svědomím to můžeme ignorovat a třeba chodit na jeden předmět do BIA a na druhý do BIB skupiny', 1),
	('BIB', 'BIA a BIB je čistě doporučené rozdělení, abyste se vlezli do prednáškovky. S klidným svědomím to můžeme ignorovat a třeba chodit na jeden předmět do BIA a na druhý do BIB skupiny', 1);

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

INSERT INTO CommandLog ([Group], Command, UserID, CalledAt, [FullCommand], GuildID, ChannelID)
SELECT [Group], Command, UserID, CalledAt, [FullCommand], GuildID, ChannelID FROM GrillBot.dbo.CommandLog;