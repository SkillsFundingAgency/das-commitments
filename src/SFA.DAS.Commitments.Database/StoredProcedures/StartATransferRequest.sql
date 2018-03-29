CREATE PROCEDURE [dbo].[StartATransferRequest]
	@commitmentid BIGINT,
	@cost MONEY,
	@trainingCourses NVARCHAR(MAX),
	@transferRequestId BIGINT = null OUT
AS
BEGIN

	DECLARE @OldApprovalStatus AS TINYINT = NULL

	INSERT INTO [dbo].[TransferRequest] (CommitmentId, Cost, TrainingCourses, [Status]) VALUES (@commitmentid, @cost, @trainingCourses, 0)

	SELECT @TransferRequestId = @@IDENTITY

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Failed to Insert TransferRequest record', 1;
	
	UPDATE [dbo].[Commitment] SET 
		@OldApprovalStatus = TransferApprovalStatus,
		TransferApprovalStatus = 0
	WHERE Id = @commitmentid;

	IF @@ROWCOUNT != 1 
		THROW 50001, 'Incorrect Number of Commitment records were updated', 1;

	IF ISNULL(@OldApprovalStatus,0) != 0
		THROW 50002, 'Approval Status on the Commitment must be Pending', 1;
END