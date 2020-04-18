CREATE TABLE [dbo].[AutoReply]
(
	[ID] INT NOT NULL PRIMARY KEY IDENTITY (1,1),
	[MustContains] VARCHAR(MAX) NOT NULL,
	[ReplyMessage] VARCHAR(MAX) NOT NULL,
	[IsDisabled] BIT NOT NULL,
	[CompareType] INT NOT NULL CONSTRAINT [DF_AutoReply_CompareType] DEFAULT (0),
	[CaseSensitive] BIT NOT NULL CONSTRAINT [DF_AutoReply_CaseSensitive] DEFAULT (0),
	[GuildID] VARCHAR(30) NULL
)
