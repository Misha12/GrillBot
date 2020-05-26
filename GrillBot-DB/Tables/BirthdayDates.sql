CREATE TABLE [dbo].[BirthdayDates]
(
	[UserID] BIGINT NOT NULL PRIMARY KEY,
	[Date] DATE NOT NULL,
	[AcceptAge] BIT NOT NULL CONSTRAINT [DF_BirthdayDates_AcceptAge] DEFAULT (0)

	CONSTRAINT FK_BirthdayDates_UserID FOREIGN KEY ([UserID]) REFERENCES [DiscordUsers]([ID])
)
