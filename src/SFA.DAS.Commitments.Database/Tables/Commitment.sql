CREATE TABLE [dbo].[Commitment]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, -- This may be a problem, I believe each time we deploy, the PK index is rebuilt (if the table changes)
    [Reference] NVARCHAR(100) NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [ProviderId] BIGINT NULL,
    [CommitmentStatus] SMALLINT NOT NULL DEFAULT 0, 
    [EditStatus] SMALLINT NOT NULL DEFAULT 0,
    [CreatedOn] DATETIME NULL, 
    [LastAction] SMALLINT NOT NULL DEFAULT 0, 
	[LastUpdatedByEmployerName] NVARCHAR(255) NULL,
    [LastUpdatedByEmployerEmail] NVARCHAR(255) NULL, 
    [LastUpdatedByProviderName] NVARCHAR(255) NULL, 
    [LastUpdatedByProviderEmail] NVARCHAR(255) NULL,
    [TransferSenderId] BIGINT SPARSE,
	[TransferApprovalStatus] TINYINT SPARSE,
	[TransferApprovalActionedByEmployerName] NVARCHAR(255),
	[TransferApprovalActionedByEmployerEmail] NVARCHAR(255),
	[TransferApprovalActionedOn] DATETIME2,
	[Originator] TINYINT NOT NULL DEFAULT 0,
	[ApprenticeshipEmployerTypeOnApproval] TINYINT NULL,
	[IsFullApprovalProcessed] BIT NOT NULL DEFAULT 0,
	[IsDeleted] BIT NOT NULL DEFAULT 0, 
    [AccountLegalEntityId] BIGINT NULL,
	[IsDraft] BIT NOT NULL DEFAULT 1,
	[WithParty] SMALLINT NOT NULL DEFAULT 0,
	[RowVersion] ROWVERSION NOT NULL,
	[LastUpdatedOn] DATETIME2 DEFAULT GETDATE() NOT NULL,
	[Approvals] SMALLINT NOT NULL DEFAULT 0,
	[EmployerAndProviderApprovedOn] DATETIME2 NULL,
	[ChangeOfPartyRequestId] BIGINT NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId_CommitmentStatus] 
ON [dbo].[Commitment] ([ProviderId], [CommitmentStatus]) 
INCLUDE ([ApprenticeshipEmployerTypeOnApproval], [CreatedOn], [EditStatus], [EmployerAccountId], [IsFullApprovalProcessed], [LastAction], [LastUpdatedByEmployerEmail], [LastUpdatedByEmployerName], [LastUpdatedByProviderEmail], [LastUpdatedByProviderName], [Originator], [Reference], [TransferApprovalActionedByEmployerEmail], [TransferApprovalActionedByEmployerName], [TransferApprovalActionedOn], [TransferApprovalStatus], [TransferSenderId]) WITH (ONLINE = ON)

GO

-- but we have 2 natural id's, providerid and EmployerAccountId
-- do we need an equivalent index for employeraccountid with all the fields included?
-- we compromise with index on EmployerAccountId (which we had before), but add in CommitmentStatus, and don't have all the columns included
--todo: reasses these indexes when they're in live, by liasing with devops to see what's recommended
CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountId_CommitmentStatus]
ON [dbo].[Commitment] ([EmployerAccountId], [CommitmentStatus]) 
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId_IsDeleted_CommitmentStatus]
ON [dbo].[Commitment] ([ProviderId], [CommitmentStatus], [IsDeleted]) 
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_TransferSenderId] ON [dbo].[Commitment] ([TransferSenderId]) WHERE [TransferSenderId] IS NOT NULL 
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderIsDeleted] on [dbo].[Commitment] ([ProviderId], [IsDeleted]) INCLUDE(
    [Reference],
    [EmployerAccountId] , 
    [CommitmentStatus] , 
    [EditStatus],
    [CreatedOn] , 
    [LastAction] , 
	[LastUpdatedByEmployerName] ,
    [LastUpdatedByEmployerEmail] , 
    [LastUpdatedByProviderName] , 
    [LastUpdatedByProviderEmail] ,
    [TransferSenderId] ,
	[TransferApprovalStatus] ,
	[TransferApprovalActionedByEmployerName] ,
	[TransferApprovalActionedByEmployerEmail] ,
	[TransferApprovalActionedOn] ,
	[Originator] ,
	[ApprenticeshipEmployerTypeOnApproval] ,
	[IsFullApprovalProcessed] ,
    [AccountLegalEntityId] ) WITH (ONLINE = ON)
	GO

CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountIsDeleted] on [dbo].[Commitment] ([EmployerAccountId], [IsDeleted]) INCLUDE(
    [Reference],
    
    [ProviderId] , 
    [CommitmentStatus] , 
    [EditStatus],
    [CreatedOn] , 
    [LastAction] , 
	[LastUpdatedByEmployerName] ,
    [LastUpdatedByEmployerEmail] , 
    [LastUpdatedByProviderName] , 
    [LastUpdatedByProviderEmail] ,
    [TransferSenderId] ,
	[TransferApprovalStatus] ,
	[TransferApprovalActionedByEmployerName] ,
	[TransferApprovalActionedByEmployerEmail] ,
	[TransferApprovalActionedOn] ,
	[Originator] ,
	[ApprenticeshipEmployerTypeOnApproval] ,
	[IsFullApprovalProcessed] ,
    [AccountLegalEntityId] ) WITH (ONLINE = ON)
	GO

CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountDeletedProviderName_Filter] ON [dbo].[Commitment] ([EmployerAccountId],[IsDeleted]) WITH (ONLINE=ON)
GO
