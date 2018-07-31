CREATE PROCEDURE [dbo].[UpdateApprenticeshipStatusesForCommitment]  
 @commitmentId BIGINT,  
 @paymentStatus SMALLINT = NULL,  
 @agreementStatus SMALLINT = NULL,
 @agreedOn DATETIME = NULL
AS  
  
UPDATE [dbo].[Apprenticeship]   
 SET
 PaymentStatus = ISNULL(@paymentStatus,PaymentStatus),
 AgreementStatus = ISNULL(@agreementStatus, AgreementStatus),
 AgreedOn = ISNULL(@agreedOn, AgreedOn)
WHERE CommitmentId = @commitmentId