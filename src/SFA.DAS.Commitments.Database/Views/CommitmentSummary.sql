CREATE VIEW [dbo].[CommitmentSummary]
AS 

SELECT c.*, 
	m.CommitmentId, m.Text, m.CreatedDateTime, m.Author, m.CreatedBy,
	COUNT(a.Id) AS ApprenticeshipCount,

	COALESCE((SELECT TOP 1 AgreementStatus FROM ApprenticeshipSummary WHERE CommitmentId = c.Id), 0) AS AgreementStatus, -- because should all be same value

	CASE
		WHEN COUNT(a.Id) > 0 AND 1 = ALL (SELECT EmployerCanApproveApprenticeship FROM ApprenticeshipSummary WHERE CommitmentId = c.Id) 
		THEN 1
		ELSE 0 
	END AS EmployerCanApproveCommitment,

	CASE
		WHEN COUNT(a.Id) > 0 AND 1 = ALL (SELECT ProviderCanApproveApprenticeship FROM ApprenticeshipSummary WHERE CommitmentId = c.Id) 
		THEN 1 
		ELSE 0 
	END AS ProviderCanApproveCommitment

FROM 
	Commitment c
LEFT JOIN [Message] m ON m.CommitmentId = c.Id
LEFT JOIN 
	Apprenticeship a ON a.CommitmentId = c.Id

GROUP BY 
	c.Id, c.Reference, c.EmployerAccountId, c.LegalEntityId, c.LegalEntityName, c.ProviderId, c.ProviderName, c.CommitmentStatus, c.EditStatus, c.CreatedOn, c.LastAction, c.LastUpdatedByEmployerEmail, c.LastUpdatedByProviderEmail, c.LastUpdatedByEmployerName, c.LastUpdatedByProviderName, c.LegalEntityAddress, c.LegalEntityOrganisationType, m.CommitmentId, m.Text, m.CreatedDateTime, m.Author, m.CreatedBy