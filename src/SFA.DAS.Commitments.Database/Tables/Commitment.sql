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

-- there are a lot of indexes on similar columns but the order does matter; mostly these indexes have been flagged as Azure recomendations
CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId_CommitmentStatus] ON [dbo].[Commitment] ([ProviderId], [CommitmentStatus]) INCLUDE(
	[ApprenticeshipEmployerTypeOnApproval], 
	[CreatedOn], 
	[EditStatus],
	[EmployerAccountId], 
	[IsFullApprovalProcessed], 
	[LastAction], 
	[LastUpdatedByEmployerEmail], 
	[LastUpdatedByEmployerName], 
	[LastUpdatedByProviderEmail], 
	[LastUpdatedByProviderName], 
	[Originator], 
	[Reference], 
	[TransferApprovalActionedOn], 
	[TransferApprovalStatus], 
	[TransferSenderId] ) 
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_IsDeleted_ProviderId_CommitmentStatus] ON [dbo].[Commitment] ([IsDeleted], [ProviderId], [CommitmentStatus]) INCLUDE (
	[AccountLegalEntityId], 
	[ApprenticeshipEmployerTypeOnApproval], 
	[Approvals], 
	[ChangeOfPartyRequestId], 
	[CreatedOn], 
	[EditStatus], 
	[EmployerAccountId], 
	[EmployerAndProviderApprovedOn], 
	[IsDraft], 
	[IsFullApprovalProcessed], 
	[LastAction], 
	[LastUpdatedByEmployerEmail], 
	[LastUpdatedByEmployerName], 
	[LastUpdatedByProviderEmail], 
	[LastUpdatedByProviderName],
	[LastUpdatedOn], 
	[Originator], 
	[Reference], 
	[RowVersion], 
	[TransferApprovalActionedOn], 
	[TransferApprovalStatus], 
	[TransferSenderId], 
	[WithParty] ) 
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId_IsDeleted_CommitmentStatus] ON [dbo].[Commitment] ([ProviderId], [CommitmentStatus], [IsDeleted]) 
WITH (ONLINE=ON)
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
	[TransferApprovalActionedOn] ,
	[Originator] ,
	[ApprenticeshipEmployerTypeOnApproval] ,
	[IsFullApprovalProcessed] ,
    [AccountLegalEntityId] ) 
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountId_CommitmentStatus] ON [dbo].[Commitment] ([EmployerAccountId], [CommitmentStatus]) 
WITH (ONLINE=ON)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountIsDeleted] on [dbo].[Commitment] ([EmployerAccountId], [IsDeleted]) INCLUDE(
    [Reference],
    [ProviderId], 
    [CommitmentStatus], 
    [EditStatus],
    [CreatedOn], 
    [LastAction], 
	[LastUpdatedByEmployerName],
    [LastUpdatedByEmployerEmail], 
    [LastUpdatedByProviderName], 
    [LastUpdatedByProviderEmail],
    [TransferSenderId],
	[TransferApprovalStatus],
	[TransferApprovalActionedOn],
	[Originator],
	[ApprenticeshipEmployerTypeOnApproval],
	[IsFullApprovalProcessed],
    [AccountLegalEntityId] ) 
WITH (ONLINE = ON)
GO

-- this looks like a duplicate however there are cases where having an index on the same columns but with no includes would be useful
CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountDeletedProviderName_Filter] ON [dbo].[Commitment] ([EmployerAccountId],[IsDeleted]) 
WITH (ONLINE=ON)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_TransferSenderId] ON [dbo].[Commitment] ([TransferSenderId]) WHERE [TransferSenderId] IS NOT NULL 
WITH (ONLINE=ON)
GO