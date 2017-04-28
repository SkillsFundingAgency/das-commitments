CREATE PROCEDURE [dbo].[UpdateApprenticeshipStatuses]
	@id BIGINT,
	@paymentStatus SMALLINT,
	@agreementStatus SMALLINT,
	@agreedOn DATETIME
AS

UPDATE [dbo].[Apprenticeship] 
	SET PaymentStatus = @paymentStatus, 
		AgreementStatus = @agreementStatus, 
		AgreedOn = @agreedOn
WHERE Id = @id