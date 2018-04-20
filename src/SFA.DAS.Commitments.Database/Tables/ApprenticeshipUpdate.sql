CREATE TABLE [dbo].[ApprenticeshipUpdate]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
	[ApprenticeshipId] BIGINT NOT NULL,
	[Originator] TINYINT NOT NULL,
	[Status] TINYINT NOT NULL DEFAULT(0),
    [FirstName] NVARCHAR(100) NULL, 
    [LastName] NVARCHAR(100) NULL, 
    [TrainingType] INT NULL, 
    [TrainingCode] NVARCHAR(20) NULL, 
    [TrainingName] NVARCHAR(126) NULL, 
    [Cost] DECIMAL NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [DateOfBirth] DATETIME NULL,
	[CreatedOn] DATETIME NULL,
	[UpdateOrigin] TINYINT NULL,
	[EffectiveFromDate] DATETIME NULL,
	[EffectiveToDate] DATETIME NULL
)
GO

CREATE INDEX [IDX_APPRENTICESHIPUPDATE_APPRENTICESHIPID] ON [dbo].[ApprenticeshipUpdate] ([ApprenticeshipId]) WITH(ONLINE=ON)
GO

CREATE NONCLUSTERED INDEX [IX_ApprenticeshipUpdate_Status] ON [dbo].[ApprenticeshipUpdate] ([Status]) INCLUDE ([ApprenticeshipId],[Originator]) WITH (ONLINE = ON)
GO