CREATE TABLE [dbo].[DiscordUsers]
(
	[ID] BIGINT NOT NULL PRIMARY KEY IDENTITY(1, 1),
	[UserID] VARCHAR(30) NOT NULL,
	[GuildID] VARCHAR(30) NOT NULL, 
	[Points] BIGINT NOT NULL CONSTRAINT DF_DiscordUsers_Points DEFAULT (0.0),
	[GivenReactionsCount] BIGINT NOT NULL CONSTRAINT DF_DiscordUsers_GivenReactionsCount DEFAULT (0),
	[ObtainedReactionsCount] BIGINT NOT NULL CONSTRAINT DF_DiscordUsers_ObtainedReactionsCount DEFAULT (0),
	[WebAdminPassword] VARCHAR(MAX) NULL,
	[ApiToken] VARCHAR(50) NULL
)

GO
CREATE NONCLUSTERED INDEX IX_DiscordUsers_UserID ON [dbo].[DiscordUsers] ([UserID]);