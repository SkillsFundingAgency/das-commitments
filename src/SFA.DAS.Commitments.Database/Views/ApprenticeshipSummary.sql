CREATE VIEW [dbo].[ApprenticeshipSummary]
AS 

SELECT 
	c.EmployerAccountId, c.ProviderId, c.Reference, 
	a.*,
	CASE 
		WHEN
			a.FirstName IS NOT NULL AND 
			a.LastName IS NOT NULL AND 
			a.Cost IS NOT NULL AND 
			a.StartDate IS NOT NULL AND 
			a.EndDate IS NOT NULL AND 
			a.TrainingCode IS NOT NULL AND 
			a.DateOfBirth IS NOT NULL AND 
			a.NINumber IS NOT NULL
		THEN
			1
		ELSE
			0
	END AS 'EmployerCanApproveApprenticeship',
	CASE 
		WHEN
			a.FirstName IS NOT NULL AND 
			a.LastName IS NOT NULL AND 
			a.ULN IS NOT NULL AND -- ULN is required for provider approval
			a.Cost IS NOT NULL AND 
			a.StartDate IS NOT NULL AND 
			a.EndDate IS NOT NULL AND 
			a.TrainingCode IS NOT NULL AND 
			a.DateOfBirth IS NOT NULL AND 
			a.NINumber IS NOT NULL
		THEN
			1
		ELSE
			0
	END AS 'ProviderCanApproveApprenticeship'

	FROM 
		Apprenticeship a
	INNER JOIN 
		Commitment c
	ON 
		c.Id = a.CommitmentId
