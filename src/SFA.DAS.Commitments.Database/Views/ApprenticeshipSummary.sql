
CREATE VIEW [dbo].[ApprenticeshipSummary]
AS 

SELECT 
	c.EmployerAccountId, c.ProviderId, c.Reference, 
	a.*, CAST(
	(
		SELECT COUNT(*)
		FROM
			Apprenticeship apps
		WHERE 
			apps.id = a.id AND 
			apps.FirstName IS NOT NULL AND 
			apps.LastName IS NOT NULL AND 
			apps.Cost IS NOT NULL AND 
			apps.StartDate IS NOT NULL AND 
			apps.EndDate IS NOT NULL AND 
			apps.TrainingCode IS NOT NULL AND 
			apps.DateOfBirth IS NOT NULL AND 
			apps.NINumber IS NOT NULL
		) AS BIT) AS 'CanApproveApprenticeship'

	FROM Apprenticeship a
	INNER JOIN 
		Commitment c
	ON 
		c.Id = a.CommitmentId
