CREATE PROCEDURE [dbo].[UpdateCommitment]
	@id BIGINT,
	@commitmentStatus SMALLINT,
	@editStatus SMALLINT,
	@lastAction SMALLINT,
	@transferApprovalStatus SMALLINT,
	@lastUpdatedByEmployerName NVARCHAR(255),
	@lastUpdatedByEmployerEmail NVARCHAR(255),
	@lastUpdatedByProviderName NVARCHAR(255),
	@lastUpdatedByProviderEmail NVARCHAR(255)
AS

UPDATE [dbo].[Commitment] 
	SET CommitmentStatus = @commitmentStatus, 
		EditStatus = @editStatus,
		LastAction = @lastAction,
		TransferApprovalStatus = @transferApprovalStatus,
		LastUpdatedByEmployerName = @lastUpdatedByEmployerName, 
		LastUpdatedByEmployerEmail = @lastUpdatedByEmployerEmail,
		LastUpdatedByProviderName = @lastUpdatedByProviderName,
		LastUpdatedByProviderEmail = @lastUpdatedByProviderEmail
WHERE Id = @id;
