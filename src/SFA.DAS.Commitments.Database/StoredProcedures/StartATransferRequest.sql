CREATE PROCEDURE [dbo].[StartATransferRequest]
	@commitmentid BIGINT,
	@cost MONEY,
	@trainingCourses NVARCHAR(MAX)
AS
BEGIN

	INSERT INTO [dbo].[TransferRequest] (CommitmentId, Cost, TrainingCourses, [Status]) VALUES (@commitmentid, @cost, @trainingCourses, 0)

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Failed to Insert TransferRequest record', 1;
	
	UPDATE [dbo].[Commitment] SET 
		TransferApprovalStatus = 0
	WHERE Id = @commitmentid;

	IF @@ROWCOUNT != 1 
		THROW 50000, 'Incorrect Number of Commitment records were updated', 1;

END