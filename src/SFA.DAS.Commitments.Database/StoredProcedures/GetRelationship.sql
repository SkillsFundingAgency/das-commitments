CREATE PROCEDURE [dbo].[GetRelationship]
	@EmployerAccountId BIGINT,
	@ProviderId BIGINT,
	@LegalEntityId nvarchar(50)
AS

	select
	Id,
	ProviderId,
	ProviderName,
	EmployerAccountId,
	LegalEntityId,
	LegalEntityName,
	Verified
	from
	[dbo].[Relationship]
	where
	EmployerAccountId = @EmployerAccountId
	and ProviderId = @ProviderId
	and LegalEntityId = @LegalEntityId

