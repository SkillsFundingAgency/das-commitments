CREATE PROCEDURE [dbo].[CreateRelationship]
(
	@ProviderId BIGINT,
	@ProviderName NVARCHAR(100),
	@EmployerAccountId BIGINT,
	@LegalEntityId NVARCHAR(50),
	@LegalEntityName NVARCHAR(100),
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
	@Verified,
	GETDATE()
)

SELECT SCOPE_IDENTITY()