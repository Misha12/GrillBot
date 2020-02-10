﻿CREATE TABLE [dbo].[MethodsConfig]
(
	[ID] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	[GuildID] VARCHAR(30) NOT NULL,
	[Group] VARCHAR(100) NOT NULL,
	[Command] VARCHAR(100) NOT NULL,
	[ConfigData] VARCHAR(MAX) NOT NULL,
	[PMAllowed] BIT NOT NULL CONSTRAINT [DF_MethodsConfig_PMAllowed] DEFAULT (0),
	[OnlyAdmins] BIT NOT NULL CONSTRAINT [DF_MethodsConfig_OnlyAdmins] DEFAULT (0)
);
