CREATE TABLE [dbo].[Commitment]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, -- This may be a problem, I believe each time we deploy, the PK index is rebuilt (if the table changes)
    [Reference] NVARCHAR(100) NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [LegalEntityId] NVARCHAR(50) NOT NULL, 
    [LegalEntityName] NVARCHAR(100) NOT NULL, 
	[LegalEntityAddress] NVARCHAR(256) NOT NULL,
	[LegalEntityOrganisationType] TINYINT NOT NULL,
    [ProviderId] BIGINT NULL,
    [ProviderName] NVARCHAR(100) NULL,
    [CommitmentStatus] SMALLINT NOT NULL DEFAULT 0, 
    [EditStatus] SMALLINT NOT NULL DEFAULT 0,
    [CreatedOn] DATETIME NULL, 
    [LastAction] SMALLINT NOT NULL DEFAULT 0, 
	[LastUpdatedByEmployerName] NVARCHAR(255) NULL,
    [LastUpdatedByEmployerEmail] NVARCHAR(255) NULL, 
    [LastUpdatedByProviderName] NVARCHAR(255) NULL, 
    [LastUpdatedByProviderEmail] NVARCHAR(255) NULL,
    [TransferSenderId] BIGINT SPARSE,
    [TransferSenderName] NVARCHAR(100) SPARSE,
	[TransferApprovalStatus] TINYINT SPARSE,
	[TransferApprovalActionedByEmployerName] NVARCHAR(255),
	[TransferApprovalActionedByEmployerEmail] NVARCHAR(255),
	[TransferApprovalActionedOn] DATETIME2,
	[AccountLegalEntityPublicHashedId] CHAR(6) NULL,
	[Originator] TINYINT NOT NULL DEFAULT 0,
	[ApprenticeshipEmployerTypeOnApproval] TINYINT NULL,
	[IsFullApprovalProcessed] BIT NOT NULL DEFAULT 0,
	[IsDeleted] BIT NOT NULL DEFAULT 0, 
    [AccountLegalEntityId] BIGINT NULL,
	[IsDraft] BIT NOT NULL DEFAULT 1,
	[WithParty] SMALLINT NOT NULL DEFAULT 0,
	[RowVersion] ROWVERSION NOT NULL,
	[LastUpdatedOn] DATETIME2 DEFAULT GETDATE() NOT NULL
	[Approvals] SMALLINT NOT NULL DEFAULT 0,
	[EmployerAndProviderApprovedOn] DATETIME2 NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId_CommitmentStatus] 
ON [dbo].[Commitment] ([ProviderId], [CommitmentStatus]) 
INCLUDE ([AccountLegalEntityPublicHashedId], [ApprenticeshipEmployerTypeOnApproval], [CreatedOn], [EditStatus], [EmployerAccountId], [IsFullApprovalProcessed], [LastAction], [LastUpdatedByEmployerEmail], [LastUpdatedByEmployerName], [LastUpdatedByProviderEmail], [LastUpdatedByProviderName], [LegalEntityAddress], [LegalEntityId], [LegalEntityName], [LegalEntityOrganisationType], [Originator], [ProviderName], [Reference], [TransferApprovalActionedByEmployerEmail], [TransferApprovalActionedByEmployerName], [TransferApprovalActionedOn], [TransferApprovalStatus], [TransferSenderId], [TransferSenderName]) WITH (ONLINE = ON)

GO

-- but we have 2 natural id's, providerid and EmployerAccountId
-- do we need an equivalent index for employeraccountid with all the fields included?
-- we compromise with index on EmployerAccountId (which we had before), but add in CommitmentStatus, and don't have all the columns included
--todo: reasses these indexes when they're in live, by liasing with devops to see what's recommended
CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountId_CommitmentStatus]
ON [dbo].[Commitment] ([EmployerAccountId], [CommitmentStatus]) 
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_TransferSenderId] ON [dbo].[Commitment] ([TransferSenderId]) WHERE [TransferSenderId] IS NOT NULL 
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderIsDeleted] on [dbo].[Commitment] ([ProviderId], [IsDeleted]) INCLUDE(
    [Reference],
    [EmployerAccountId] , 
    [LegalEntityId] ,
    [LegalEntityName] , 
	[LegalEntityAddress] ,
	[LegalEntityOrganisationType] ,
    [ProviderName] ,
    [CommitmentStatus] , 
    [EditStatus],
    [CreatedOn] , 
    [LastAction] , 
	[LastUpdatedByEmployerName] ,
    [LastUpdatedByEmployerEmail] , 
    [LastUpdatedByProviderName] , 
    [LastUpdatedByProviderEmail] ,
    [TransferSenderId] ,
    [TransferSenderName] ,
	[TransferApprovalStatus] ,
	[TransferApprovalActionedByEmployerName] ,
	[TransferApprovalActionedByEmployerEmail] ,
	[TransferApprovalActionedOn] ,
	[AccountLegalEntityPublicHashedId] ,
	[Originator] ,
	[ApprenticeshipEmployerTypeOnApproval] ,
	[IsFullApprovalProcessed] ,
    [AccountLegalEntityId] ) WITH (ONLINE = ON)
	GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderDeletedLegalEntityName_Filter] ON [dbo].[Commitment] ([ProviderId],[IsDeleted]) INCLUDE ([LegalEntityName]) WITH (ONLINE=ON)
GO
