CREATE PROCEDURE [dbo].[VerifyRelationship]
	@EmployerAccountId BIGINT,
	@ProviderId BIGINT,
	@LegalEntityId nvarchar(50)
AS

	update
	[dbo].Relationship
	set Verified = 1
	where
	EmployerAccountId = @EmployerAccountId
	and ProviderId = @ProviderId
	and LegalEntityId = @LegalEntityId
