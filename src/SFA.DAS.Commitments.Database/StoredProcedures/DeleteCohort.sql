CREATE PROCEDURE [dbo].[DeleteCohort]
	@commitmentId BIGINT
AS
	BEGIN 
		SET NOCOUNT ON

		BEGIN TRY
			BEGIN TRANSACTION
	    
			DELETE FROM [dbo].[Apprenticeship] WHERE CommitmentId = @commitmentId

			DELETE FROM [dbo].[Commitment] WHERE Id = @commitmentId

			COMMIT TRANSACTION

		END TRY
		BEGIN CATCH
			DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
				@ErrNum INT = ERROR_NUMBER(),
				@ErrProc NVARCHAR(126) = ERROR_PROCEDURE()
			
			DECLARE @DataError NVARCHAR(4000) = 'Error deleting cohort'
				+ CONVERT(NVARCHAR(10), @ErrNum) + ', Error Details: '
				+ @ErrMsg

			ROLLBACK TRANSACTION
			RAISERROR (@DataError, 16, 1)
		END CATCH
	END
GO
