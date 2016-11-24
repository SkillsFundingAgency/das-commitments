CREATE VIEW [dbo].[CommitmentSummary]
	AS SELECT c.*, 
		COUNT(a.CommitmentId) AS ApprenticeshipCount,
		COALESCE((SELECT TOP 1 app.AgreementStatus FROM Apprenticeship app WHERE app.CommitmentId = c.Id), 0) AS AgreementStatus
	FROM 
		Commitment c
	LEFT JOIN 
		Apprenticeship a ON a.CommitmentId = c.Id
	GROUP BY 
		c.Id, c.Reference, c.EmployerAccountId, c.LegalEntityId, c.LegalEntityName, c.ProviderId, c.ProviderName, c.CommitmentStatus, c.EditStatus
