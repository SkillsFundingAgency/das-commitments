CREATE PROCEDURE [dbo].[UpdateCommitment]
	@id BIGINT,
	@commitmentStatus SMALLINT,
	@editStatus SMALLINT,
	@lastAction SMALLINT,
	@transferApprovalStatus SMALLINT,
	@lastUpdatedByEmployerName NVARCHAR(255),
	@lastUpdatedByEmployerEmail NVARCHAR(255),
	@lastUpdatedByProviderName NVARCHAR(255),
	@lastUpdatedByProviderEmail NVARCHAR(255),
	@apprenticeshipEmployerTypeOnApproval TINYINT = NULL
AS
BEGIN

	UPDATE [dbo].[Commitment] 
		SET CommitmentStatus = @commitmentStatus, 
			EditStatus = @editStatus,
			LastAction = @lastAction,
			TransferApprovalStatus = @transferApprovalStatus,
			LastUpdatedByEmployerName = @lastUpdatedByEmployerName, 
			LastUpdatedByEmployerEmail = @lastUpdatedByEmployerEmail,
			LastUpdatedByProviderName = @lastUpdatedByProviderName,
			LastUpdatedByProviderEmail = @lastUpdatedByProviderEmail,
			ApprenticeshipEmployerTypeOnApproval = @apprenticeshipEmployerTypeOnApproval,
			Approvals = 0
	WHERE Id = @id
	AND EditStatus <> 0;


	IF @@ROWCOUNT != 1 
		THROW 50003, 'Incorrect Number of records were updated in Commitment', 1;

END