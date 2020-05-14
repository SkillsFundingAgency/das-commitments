CREATE PROCEDURE [dbo].[DeleteCommitment]
	@commitmentId BIGINT
AS
	BEGIN 
			DELETE FROM [dbo].[Apprenticeship] WHERE CommitmentId = @commitmentId

			DELETE FROM [dbo].[Message] WHERE CommitmentId = @commitmentId

			update [dbo].[Commitment] set IsDeleted=1 WHERE Id = @commitmentId

			--Also mark COPR as Withdrawn or Rejected
			declare @ChangeOfPartyRequestId BIGINT
			declare @WithParty BIGINT
			select @ChangeOfPartyRequestId = ChangeOfPartyRequestId, @WithParty = WithParty from Commitment where Id = @commitmentId

			if(@ChangeOfPartyRequestId is not null)
			BEGIN

				update [dbo].[ChangeOfPartyRequest]
				set [Status] = CASE WHEN OriginatingParty = @WithParty THEN 2 ELSE 1 END
				where Id = @ChangeOfPartyRequestId

			END
	END
GO
