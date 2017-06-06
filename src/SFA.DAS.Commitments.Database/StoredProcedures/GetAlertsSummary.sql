CREATE PROCEDURE [dbo].[GetAlertsSummary]
AS

SELECT 
	EmployerAccountId
	, COUNT(*) AS TotalCount
	, COUNT(CASE UpdateOriginator WHEN 1 THEN 1 ELSE NULL END) AS ChangeOfCircCount
	, COUNT(CASE DataLockTriage WHEN 2 THEN 1 ELSE NULL END) AS RestartRequestCount
	FROM (SELECT [Id]
				, [PaymentStatus]
				, [UpdateOriginator]
				, [DataLockTriage]
				, EmployerAccountId
			FROM [dbo].[ApprenticeshipSummary]
			WHERE PaymentStatus > 0
			AND (DataLockTriage = 2 OR UpdateOriginator = 1)
		) a
GROUP BY EmployerAccountId
ORDER BY EmployerAccountId