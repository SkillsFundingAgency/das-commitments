CREATE TABLE [dbo].[DataLockUpdaterJobHistory]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[StartedOn] DATETIME NOT NULL,
	[FinishedOn] DATETIME NOT NULL,
	[FromEventId] BIGINT NOT NULL,
	[SkippedCount] INT NOT NULL,
	[DuplicateCount] INT NOT NULL,
	[ExpiredCount] INT NOT NULL,
	[PagesRemaining] INT NOT NULL
)
