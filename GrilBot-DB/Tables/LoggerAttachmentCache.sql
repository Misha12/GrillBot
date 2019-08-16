CREATE TABLE [dbo].[LoggerAttachmentCache]
(
	[AttachmentID] VARCHAR(30) NOT NULL PRIMARY KEY,
	[MessageID] VARCHAR(30) NOT NULL,
	[UrlLink] NVARCHAR(255) NOT NULL,
	[ProxyUrl] NVARCHAR(255) NOT NULL,

	CONSTRAINT [FK_LoggerAttachmentCache] FOREIGN KEY ([MessageID]) REFERENCES [LoggerMessageCache]([MessageID])
)
