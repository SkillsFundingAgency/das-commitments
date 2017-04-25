CREATE PROCEDURE [dbo].[DeleteCommitment]
	@commitmentId BIGINT
AS
	BEGIN 
			DELETE FROM [dbo].[Apprenticeship] WHERE CommitmentId = @commitmentId

			DELETE FROM [dbo].[Message] WHERE CommitmentId = @commitmentId

			DELETE FROM [dbo].[Commitment] WHERE Id = @commitmentId
	END
GO
