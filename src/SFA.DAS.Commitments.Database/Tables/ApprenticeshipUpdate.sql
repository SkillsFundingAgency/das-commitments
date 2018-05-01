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

CREATE NONCLUSTERED INDEX [IX_ApprenticeshipUpdate_ApprenticeshipId_Status] ON [dbo].[ApprenticeshipUpdate] ([ApprenticeshipId], [Status]) INCLUDE ([Originator]) WITH (ONLINE = ON)
GO

--todo:this was recommended by azure (with the old apprenticeshipsummary view) but might be worth trying
--CREATE NONCLUSTERED INDEX [nci_wi_ApprenticeshipUpdate_97B6F3CEAF1484B61E5FC09AB1376AFF] ON [dbo].[ApprenticeshipUpdate] ([Status]) INCLUDE ([ApprenticeshipId], [Originator]) WITH (ONLINE = ON)
--GO