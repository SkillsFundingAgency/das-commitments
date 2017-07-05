CREATE PROCEDURE [dbo].[GetProviderAlertsSummary]
AS

SELECT 
	  ProviderId
	, ProviderName
	, COUNT(*) AS TotalCount
	, COUNT(CASE UpdateOriginator WHEN 0 THEN 1 ELSE NULL END) AS ChangesForReview
	, SUM(CASE WHEN (DataLockCourse = 1 OR DataLockPrice = 1) THEN 1 ELSE 0 END) as DataMismatchCount
	FROM (SELECT [Id]
				, [PaymentStatus]
				, [UpdateOriginator]
				, DataLockCourse
				, DataLockPrice
				, ProviderId
				, ProviderName
			FROM [dbo].[ApprenticeshipSummary]
			WHERE PaymentStatus > 0
			AND (DataLockCourse = 1 OR DataLockPrice = 1 OR UpdateOriginator = 0)
		) a
GROUP BY ProviderId, ProviderName
ORDER BY ProviderId