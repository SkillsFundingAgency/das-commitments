CREATE TABLE [dbo].[Commitment]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Reference] NVARCHAR(100) NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [LegalEntityId] NVARCHAR(50) NOT NULL, 
    [LegalEntityName] NVARCHAR(100) NOT NULL, 
    [ProviderId] BIGINT NULL,
    [ProviderName] NVARCHAR(100) NULL,
    [CommitmentStatus] SMALLINT NOT NULL DEFAULT 0, 
    [EditStatus] SMALLINT NOT NULL DEFAULT 0,
    [CreatedOn] DATETIME NULL, 
    [LastAction] SMALLINT NOT NULL DEFAULT 0
)