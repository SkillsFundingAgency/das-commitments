CREATE PROCEDURE [dbo].[GetApprenticeshipStatusSummaries]
	@employerAccountId BIGINT
AS

SELECT 
	ale.LegalEntityId, ale.OrganisationType as 'LegalEntityOrganisationType', a.PaymentStatus, COUNT(*) AS 'Count'
FROM Apprenticeship a
INNER JOIN Commitment c ON a.CommitmentId = c.Id
INNER JOIN [AccountLegalEntities] ale on ale.Id = c.AccountLegalEntityId
WHERE 
	c.EmployerAccountId = @employerAccountId
GROUP BY 
	ale.LegalEntityId, ale.OrganisationType, a.PaymentStatus
