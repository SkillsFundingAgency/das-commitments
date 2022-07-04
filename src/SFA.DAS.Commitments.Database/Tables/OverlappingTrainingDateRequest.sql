CREATE TABLE [dbo].[OverlappingTrainingDateRequest]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[ApprenticeshipId] BIGINT NOT NULL,
	[PreviousApprenticeshipId] BIGINT NOT NULL,
	[ResolutionType] SMALLINT NULL,
	[Status] SMALLINT NOT NULL DEFAULT 0, 
	[EmployerAction] SMALLINT NULL, 
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[ActionedOn] DATETIME2 NULL
    CONSTRAINT [FK_OverlappingTrainingDateRequest_Apprenticeship] FOREIGN KEY ([ApprenticeshipId]) REFERENCES [Apprenticeship]([Id]),
	CONSTRAINT [FK_OverlappingTrainingDateRequest_PreviousApprenticeship] FOREIGN KEY ([PreviousApprenticeshipId]) REFERENCES [Apprenticeship]([Id]),
)
