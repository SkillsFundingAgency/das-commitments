CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsForEmployer]
	@id BIGINT
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE @id = s.EmployerAccountId
	AND NOT s.PaymentStatus IN (0,5); --Not deleted or pre-approved
