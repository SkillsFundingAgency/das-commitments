CREATE PROCEDURE [dbo].[CreateRelationship]
(
	@ProviderId BIGINT,
	@ProviderName NVARCHAR(100),
	@EmployerAccountId BIGINT,
	@LegalEntityId NVARCHAR(50),
	@LegalEntityName NVARCHAR(100),
	@LegalEntityAddress NVARCHAR(256),
	@LegalEntityOrganisationType TINYINT,
	@Verified BIT
)
as

insert into [dbo].[Relationship]
(
	ProviderId,
	ProviderName,
	EmployerAccountId,
	LegalEntityId,
	LegalEntityName,
	LegalEntityAddress,
	LegalEntityOrganisationType,
	Verified,
	CreatedOn
)
values
(
	@ProviderId,
	@ProviderName,
	@EmployerAccountId,
	@LegalEntityId,
	@LegalEntityName,
	@LegalEntityAddress,
	@LegalEntityOrganisationType,
	@Verified,
	GETDATE()
)

SELECT SCOPE_IDENTITY()