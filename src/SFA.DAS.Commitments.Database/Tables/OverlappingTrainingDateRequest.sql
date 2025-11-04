CREATE TABLE [dbo].[OverlappingTrainingDateRequest]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY,
	[DraftApprenticeshipId] BIGINT NOT NULL,
	[PreviousApprenticeshipId] BIGINT NOT NULL,
	[ResolutionType] SMALLINT NULL,
	[Status] SMALLINT NOT NULL DEFAULT 0, 
	[CreatedOn] DATETIME2 NOT NULL DEFAULT GETDATE(),
	[ActionedOn] DATETIME2 NULL,
	[NotifiedServiceDeskOn] DATETIME2 NULL,
	[NotifiedEmployerOn] DATETIME2 NULL,
	[RowVersion] ROWVERSION NOT NULL,
    [RequestCreatedByProviderEmail] NVARCHAR(255) NULL,
)
GO

CREATE NONCLUSTERED INDEX [IX_OverlappingTrainingDateRequest_PreviousApprenticeshipId_Status] ON [dbo].[OverlappingTrainingDateRequest] ([PreviousApprenticeshipId], [Status]) INCLUDE (
     [ActionedOn], 
     [CreatedOn], 
     [DraftApprenticeshipId], 
     [NotifiedEmployerOn], 
     [NotifiedServiceDeskOn], 
     [RequestCreatedByProviderEmail], 
     [ResolutionType], 
     [RowVersion]) 
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_OverlappingTrainingDateRequest_DraftApprenticeshipId_Status] 
ON [dbo].[OverlappingTrainingDateRequest] ([DraftApprenticeshipId], [Status]) INCLUDE (
     [PreviousApprenticeshipId]) 
WITH (ONLINE = ON)
GO