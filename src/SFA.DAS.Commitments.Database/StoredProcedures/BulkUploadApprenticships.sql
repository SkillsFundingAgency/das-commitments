CREATE PROCEDURE [dbo].[BulkUploadApprenticships]
	@commitmentId BIGINT,
	@apprenticeships ApprenticeshipTable READONLY
AS
	BEGIN
		SET NOCOUNT ON;

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
	END
GO
