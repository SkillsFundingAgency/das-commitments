CREATE PROCEDURE [dbo].[VerifyRelationship]
	@EmployerAccountId BIGINT,
	@ProviderId BIGINT,
	@LegalEntityId nvarchar(50),
	@Verified BIT
AS

	update
	[dbo].Relationship
	set Verified = @Verified
	where
	EmployerAccountId = @EmployerAccountId
	and ProviderId = @ProviderId
	and LegalEntityId = @LegalEntityId
