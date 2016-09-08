CREATE TABLE [dbo].[Commitment]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(100) NOT NULL, 
    [EmployerAccountId] BIGINT NOT NULL, 
    [LegalEntityId] BIGINT NOT NULL, 
    [LegalEntityName] NVARCHAR(100) NOT NULL, 
    [ProviderId] BIGINT NULL,
    [ProviderName] NVARCHAR(100) NULL 
)
