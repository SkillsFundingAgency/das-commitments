CREATE PROCEDURE [dbo].[GetApprenticeshipStatusSummaries]
	@employerAccountId BIGINT
AS

SELECT 
	c.LegalEntityId, c.LegalEntityOrganisationType, a.PaymentStatus, COUNT(*) AS 'Count'
FROM 
	Apprenticeship a
INNER JOIN 
	Commitment c
ON 
	a.CommitmentId = c.Id
WHERE 
	c.EmployerAccountId = @employerAccountId
GROUP BY 
	c.LegalEntityId, c.LegalEntityOrganisationType, a.PaymentStatus
