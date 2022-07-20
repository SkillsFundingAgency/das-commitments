CREATE TABLE [dbo].[OverlappingTrainingDateRequest]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[DraftApprenticeshipId] BIGINT NOT NULL,
	[PreviousApprenticeshipId] BIGINT NOT NULL,
	[ResolutionType] SMALLINT NULL,
	[Status] SMALLINT NOT NULL DEFAULT 0, 
	[EmployerAction] SMALLINT NULL, 
	[CreatedOn] DATETIME NOT NULL DEFAULT GETDATE(),
	[ActionedOn] DATETIME2 NULL,
	[RowVersion] ROWVERSION NOT NULL
   
)
