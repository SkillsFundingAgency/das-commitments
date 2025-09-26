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
    [TrainingCourseVersion] NVARCHAR(10) NULL,
    [TrainingCourseVersionConfirmed] BIT NULL,
    [StandardUId] NVARCHAR(20) NULL,
    [TrainingCourseOption] NVARCHAR(126) NULL,
    [Cost] DECIMAL NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [DateOfBirth] DATETIME NULL,
	[CreatedOn] DATETIME NULL,
	[UpdateOrigin] TINYINT NULL,
	[EffectiveFromDate] DATETIME NULL,
	[EffectiveToDate] DATETIME NULL, 
    [Email] NVARCHAR(200) NULL,
    [DeliveryModel] TINYINT NULL, 
    [EmploymentPrice] INT NULL, 
    [EmploymentEndDate] DATETIME2 NULL,
    [ActualStartDate] DATETIME NULL,
    [TrainingPrice] DECIMAL NULL, 
    [EndPointAssessmentPrice] DECIMAL NULL
)
GO

-- this isn't required for ApprenticeshipSummary use anymore, but other db work might benefit by it
CREATE NONCLUSTERED INDEX [IX_ApprenticeshipUpdate_ApprenticeshipId_Status] ON [dbo].[ApprenticeshipUpdate] ([ApprenticeshipId], [Status]) INCLUDE ([Originator]) WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IDX_ApprenticeshipUpdate_Status_Originator] ON [dbo].[ApprenticeshipUpdate] ([Status], [Originator])  INCLUDE ([ApprenticeshipId])  WITH (ONLINE = ON)
GO