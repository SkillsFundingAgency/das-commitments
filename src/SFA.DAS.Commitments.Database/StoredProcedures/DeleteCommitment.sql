CREATE PROCEDURE [dbo].[DeleteCommitment]
	@commitmentId BIGINT
AS
	BEGIN 
			DELETE FROM [dbo].[Apprenticeship] WHERE CommitmentId = @commitmentId

			DELETE FROM [dbo].[Message] WHERE CommitmentId = @commitmentId

			update [dbo].[Commitment] set IsDeleted=1 WHERE Id = @commitmentId
	END
GO
