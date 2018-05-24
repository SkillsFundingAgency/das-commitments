CREATE PROCEDURE [dbo].[SetTransferApproval]
	@id BIGINT,
	@transferApprovalStatus TINYINT,
	@transferStatusSetByEmployerName NVARCHAR(255),
	@transferStatusSetByEmployerEmail NVARCHAR(255)
AS
BEGIN

	DECLARE @OldApprovalStatus AS TINYINT = NULL
	DECLARE @ApproveDate AS DATETIME = GETDATE()  

	UPDATE [dbo].[Commitment] SET 
		@OldApprovalStatus = TransferApprovalStatus,
		TransferApprovalStatus = @transferApprovalStatus,
		TransferApprovalActionedByEmployerEmail = @transferStatusSetByEmployerEmail,
		TransferApprovalActionedByEmployerName = @transferStatusSetByEmployerName,
		TransferApprovalActionedOn = @ApproveDate
	WHERE Id = @id;

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Incorrect Number of records were updated', 1;

	IF ISNULL(@OldApprovalStatus, 0) != 0
		THROW 50001, 'Approval Status has already been updated', 1;

	-- Also find the associated TransferRequest Record and Update that as well. This will be needed until MYA start using the API to get the Pending Transfers Requests
	UPDATE [dbo].[TransferRequest] SET 
		[Status] = @transferApprovalStatus,
		TransferApprovalActionedByEmployerEmail = @transferStatusSetByEmployerEmail,
		TransferApprovalActionedByEmployerName = @transferStatusSetByEmployerName,
		TransferApprovalActionedOn = @ApproveDate
	WHERE CommitmentId = @id AND [Status] = 0

	IF @@ROWCOUNT != 1 
		THROW 50003, 'Incorrect Number of records were updated in TransferRequest', 1;

END