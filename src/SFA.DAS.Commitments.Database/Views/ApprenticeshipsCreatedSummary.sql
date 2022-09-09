CREATE VIEW [DashboardReporting].[ApprenticeshipsCreatedSummary]
	AS 
SELECT 
	CASE WHEN TransferApprovalActionedOn IS NOT NULL THEN C.TransferApprovalActionedOn
	ELSE C.EmployerAndProviderApprovedOn 
	END AS ApprovedOn, 
	A.Id, 
	ISNULL(A.DeliveryModel,0) AS DeliveryModel, 
	ISNULL(PL.IsAccelerated, 0) AS AcceleratedDelivery
FROM Apprenticeship A 
INNER JOIN Commitment C ON A.CommitmentId = C.Id
LEFT JOIN ApprenticeshipPriorLearning PL ON A.Id = PL.ApprenticeshipId
WHERE A.IsApproved = 1
GO