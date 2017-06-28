CREATE PROCEDURE [dbo].[GetProviderAlertsSummary]
AS

SELECT 
	  ProviderId
	, ProviderName
	, COUNT(*) AS TotalCount
	, COUNT(CASE UpdateOriginator WHEN 0 THEN 1 ELSE NULL END)
		AS ChangesForReview
	, COUNT(CASE DataLockCourse WHEN 1 THEN 1 ELSE NULL END) 
	+ COUNT(CASE DataLockPrice WHEN 1 THEN 1 ELSE NULL END)
		AS DataMismatchCount
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