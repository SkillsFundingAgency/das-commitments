CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsForEmployer]
	@id BIGINT
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE @id = s.EmployerAccountId
	AND NOT s.PaymentStatus IN (0,5); --Not deleted or pre-approved

	/*
	This proc should be deleted and has been retained only to prevent issues during release window
	*/