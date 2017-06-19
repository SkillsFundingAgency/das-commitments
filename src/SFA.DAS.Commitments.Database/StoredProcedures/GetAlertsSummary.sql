CREATE PROCEDURE [dbo].[GetAlertsSummary]
AS

SELECT 
	EmployerAccountId
	, COUNT(*) AS TotalCount
	, COUNT(CASE UpdateOriginator WHEN 1 THEN 1 ELSE NULL END)
		+ COUNT(CASE DataLockPriceTriaged WHEN 1 THEN 1 ELSE NULL END)
		AS ChangesForReview
	, COUNT(CASE DataLockCourseTriaged WHEN 1 THEN 1 ELSE NULL END) AS RestartRequestCount
	FROM (SELECT [Id]
				, [PaymentStatus]
				, [UpdateOriginator]
				, DataLockCourseTriaged
				, DataLockPriceTriaged
				, EmployerAccountId
			FROM [dbo].[ApprenticeshipSummary]
			WHERE PaymentStatus > 0
			AND (DataLockCourseTriaged = 1 OR DataLockPriceTriaged = 1 OR UpdateOriginator = 1)
		) a
GROUP BY EmployerAccountId
ORDER BY EmployerAccountId