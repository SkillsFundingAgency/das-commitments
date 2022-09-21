CREATE VIEW [DashboardReporting].[ApprenticeshipsWithNoEmail]
	AS 
SELECT 
	CASE WHEN TransferApprovalActionedOn IS NOT NULL THEN C.TransferApprovalActionedOn
	ELSE C.EmployerAndProviderApprovedOn 
	END AS ApprovedOn, 
	A.Id AS ApprenticeshipId
FROM Apprenticeship A
INNER JOIN Commitment C ON A.CommitmentId = C.Id
WHERE A.IsApproved = 1 AND C.EmployerAndProviderApprovedOn >= '2021-09-10' AND A.Email IS NULL AND A.ContinuationOfId IS NULL
GO
