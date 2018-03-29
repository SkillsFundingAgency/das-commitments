CREATE PROCEDURE [dbo].[SetTransferRequestApproval]
	@transferRequestId BIGINT,
	@commitmentId BIGINT,
	@transferApprovalStatus TINYINT,
	@transferStatusSetByEmployerName NVARCHAR(255),
	@transferStatusSetByEmployerEmail NVARCHAR(255)
AS
BEGIN

	DECLARE @OldApprovalStatus AS TINYINT = NULL

	UPDATE [dbo].[Commitment] SET 
		@OldApprovalStatus = TransferApprovalStatus,
		TransferApprovalStatus = @transferApprovalStatus
	WHERE Id = @commitmentId;

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Incorrect Number of Commitment records were updated', 1;

	IF @OldApprovalStatus != 0 
		THROW 51000, 'Transfer Approval Status is not in the Pending State on the Commitment Record', 1;

	UPDATE [dbo].[TransferRequest] SET 
		@OldApprovalStatus = [Status],
		[Status] = @transferApprovalStatus,
		TransferApprovalActionedByEmployerEmail = @transferStatusSetByEmployerEmail,
		TransferApprovalActionedByEmployerName = @transferStatusSetByEmployerName,
		TransferApprovalActionedOn = GETDATE()
	WHERE Id = @transferRequestId AND CommitmentId = @commitmentId;

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Incorrect Number of TransferRequest records were updated', 1;

	IF @OldApprovalStatus != 0 
		THROW 51000, 'Transfer Approval Status is not in the Pending State on the TransferRequest Record', 1;


END