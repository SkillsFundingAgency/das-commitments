CREATE PROCEDURE [dbo].[GetActiveApprenticeships]
	@providerId BIGINT = NULL,
	@employerId BIGINT = NULL
AS

SELECT 
	s.*
	FROM ApprenticeshipSummary s
	WHERE
		(@providerId IS NULL OR @providerId = s.ProviderId)
	AND
		(@employerId IS NULL OR @employerId = s.EmployerAccountId)
	AND
		NOT s.PaymentStatus IN (0,5) --Not deleted or pre-approved
	ORDER BY s.FirstName asc, s.LastName asc;