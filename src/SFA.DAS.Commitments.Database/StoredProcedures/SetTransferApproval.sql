CREATE PROCEDURE [dbo].[SetTransferApproval]
	@id BIGINT,
	@transferApprovalStatus TINYINT,
	@transferStatusSetByEmployerName NVARCHAR(255),
	@transferStatusSetByEmployerEmail NVARCHAR(255)
AS
BEGIN

	DECLARE @OldApprovalStatus AS TINYINT = NULL

	UPDATE [dbo].[Commitment] SET 
		@OldApprovalStatus = TransferApprovalStatus,
		TransferApprovalStatus = @transferApprovalStatus,
		TransferApprovalActionedByEmployerEmail = @transferStatusSetByEmployerEmail,
		TransferApprovalActionedByEmployerName = @transferStatusSetByEmployerName,
		TransferApprovalActionedOn = GETDATE()
	WHERE Id = @id;

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Incorrect Number of records were updated', 1;

	IF @OldApprovalStatus IS NOT NULL
		THROW 51000, 'Approval Status has already been updated', 1;

END