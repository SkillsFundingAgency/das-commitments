CREATE TABLE [dbo].[DataLockStatus]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[DataLockEventId] BIGINT NOT NULL,
	[DataLockEventDatetime] DATETIME NOT NULL,
    [PriceEpisodeIdentifier] NVARCHAR(25) NOT NULL,
    [ApprenticeshipId] BIGINT NOT NULL,
    [IlrTrainingCourseCode] NVARCHAR(20) NULL,
    [IlrTrainingType] TINYINT NOT NULL,
	[IlrActualStartDate] DATETIME NULL,
	[IlrEffectiveFromDate] DATETIME NULL,
	[IlrTotalCost] DECIMAL NULL,
    [ErrorCodes] INT NOT NULL,
    [Status] TINYINT NOT NULL,
    [TriageStatus] TINYINT NOT NULL,
	CONSTRAINT [FK_DataLockStatus_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_DataLockStatus_DataLockEventId] ON [dbo].[DataLockStatus] ([DataLockEventId])
GO

CREATE UNIQUE INDEX [IX_DataLockStatus_ApprenticeshipId] ON [dbo].[DataLockStatus] ([ApprenticeshipId],[PriceEpisodeIdentifier])
GO

