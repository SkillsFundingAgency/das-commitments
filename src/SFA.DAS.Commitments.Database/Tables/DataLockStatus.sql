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
	[IlrPriceEffectiveToDate] DATETIME NULL,
	[IlrTotalCost] DECIMAL NULL,
    [ErrorCode] INT NOT NULL,
    [Status] TINYINT NOT NULL,
    [TriageStatus] TINYINT NOT NULL,
	[ApprenticeshipUpdateId] BIGINT NULL,
	[IsResolved] BIT NOT NULL,
	[EventStatus] TINYINT NOT NULL DEFAULT 1,
	[IsExpired] BIT NOT NULL DEFAULT(0),
	[Expired] DATETIME NULL,
	CONSTRAINT [FK_DataLockStatus_ApprenticeshipId] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]),
	CONSTRAINT [FK_DataLockStatus_ApprenticeshipUpdateId] FOREIGN KEY ([ApprenticeshipUpdateId]) REFERENCES [ApprenticeshipUpdate]([Id])
)
GO

CREATE NONCLUSTERED INDEX [IX_DataLockStatus_DataLockEventId] ON [dbo].[DataLockStatus] ([DataLockEventId])
GO

CREATE UNIQUE INDEX [IX_DataLockStatus_ApprenticeshipId] ON [dbo].[DataLockStatus] ([ApprenticeshipId],[PriceEpisodeIdentifier])
GO

CREATE NONCLUSTERED INDEX [IX_DataLockStatus_ApprenticeshipId2]
ON [dbo].[DataLockStatus] ([ApprenticeshipId], [IsExpired], [IsResolved], [EventStatus])
INCLUDE ([DataLockEventId], [ErrorCode], [TriageStatus])
GO

CREATE NONCLUSTERED INDEX [IDX_DataLockStatus_StatusEventStatusResolvedSearch] ON [dbo].[DataLockStatus] ([Status],[IsResolved],[EventStatus],[IsExpired]) INCLUDE ([ApprenticeshipId])
GO

CREATE NONCLUSTERED INDEX [IX_DataLockStatus_IlrEffectiveExpired] ON [dbo].[DataLockStatus] ([IsExpired], [IlrEffectiveFromDate]) WITH (ONLINE = ON)
GO
