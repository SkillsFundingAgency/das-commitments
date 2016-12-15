CREATE PROCEDURE [dbo].[BulkUploadApprenticships]
	@commitmentId BIGINT,
	@apprenticeships ApprenticeshipTable READONLY
AS
	BEGIN
		SET NOCOUNT ON;
		
		BEGIN TRY
			BEGIN TRANSACTION

			DELETE FROM [dbo].[Apprenticeship] WHERE CommitmentId = @commitmentId

			INSERT [dbo].[Apprenticeship] 
				( 
					CommitmentId, 
					FirstName, 
					LastName, 
					ULN, 
					TrainingType, 
					TrainingCode, 
					TrainingName, 
					Cost, 
					StartDate, 
					EndDate, 
					AgreementStatus, 
					PaymentStatus, 
					DateOfBirth, 
					NINumber, 
					EmployerRef, 
					ProviderRef, 
					CreatedOn)
				SELECT @commitmentId, a.* 
				FROM @apprenticeships a
			
			COMMIT TRANSACTION

		END TRY
		BEGIN CATCH

			DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
				@ErrNum INT = ERROR_NUMBER(),
				@ErrProc NVARCHAR(126) = ERROR_PROCEDURE()
			DECLARE @DataError NVARCHAR(4000) = 'Error loading data to Apprenticeship table'
				+ CONVERT(NVARCHAR(10), @ErrNum) + ', Error Details: '
				+ @ErrMsg

			ROLLBACK TRANSACTION
			RAISERROR (@DataError, 16, 1)

		END CATCH
	END
GO
