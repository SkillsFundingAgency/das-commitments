﻿CREATE TABLE [dbo].[Commitment]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
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
	[LastUpdatedByEmployerName] NVARCHAR(255) NOT NULL,
    [LastUpdatedByEmployerEmail] NVARCHAR(255) NOT NULL, 
    [LastUpdatedByProviderName] NVARCHAR(255) NULL, 
    [LastUpdatedByProviderEmail] NVARCHAR(255) NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId] ON [dbo].[Commitment] ([ProviderId]) include([CommitmentStatus])
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_EmployerAccountId] ON [dbo].[Commitment] ([EmployerAccountId])
GO

CREATE NONCLUSTERED INDEX [IX_Commitment_ProviderId_Status]
ON [dbo].[Commitment] ([ProviderId],[CommitmentStatus])
INCLUDE ([Reference],[EmployerAccountId],[LegalEntityId],[LegalEntityName],[LegalEntityAddress],[LegalEntityOrganisationType],[ProviderName],[EditStatus],[CreatedOn],[LastAction],[LastUpdatedByEmployerName],[LastUpdatedByEmployerEmail],[LastUpdatedByProviderName],[LastUpdatedByProviderEmail])
GO