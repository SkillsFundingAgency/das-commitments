CREATE VIEW [dbo].[ApprenticeshipSummary]
AS 

SELECT 
	c.EmployerAccountId, c.ProviderId, c.Reference, 
	a.* 
FROM Apprenticeship a
INNER JOIN 
	Commitment c
ON 
	c.Id = a.CommitmentId
