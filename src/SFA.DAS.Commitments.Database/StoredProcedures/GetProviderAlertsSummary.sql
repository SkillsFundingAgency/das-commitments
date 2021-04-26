CREATE PROCEDURE [dbo].[GetProviderAlertsSummary]
AS


SELECT 
	  a.ProviderId
	, p.ProviderName
	, COUNT(*) AS TotalCount
	, COUNT(CASE UpdateOriginator WHEN 0 THEN 1 ELSE NULL END) AS ChangesForReview
	, SUM(CASE WHEN ((DataLockCourse = 1 OR DataLockPrice = 1) AND PaymentStatus in (1,2)) THEN 1 ELSE 0 END) as DataMismatchCount  
	FROM (SELECT [Id]
				, [PaymentStatus]
				, [UpdateOriginator]
				, DataLockCourse
				, DataLockPrice
				, ProviderId
			FROM [dbo].[ApprenticeshipSummary]
			WHERE PaymentStatus > 0
			AND (DataLockCourse = 1 OR DataLockPrice = 1 OR DataLockCourseChangeTriaged = 1 OR UpdateOriginator = 0)
		) a
	INNER JOIN (
		SELECT ProviderId, ProviderName FROM 
			(SELECT ProviderId, p.[Name] as 'ProviderName', ROW_NUMBER() OVER (PARTITION BY ProviderId ORDER BY CreatedOn DESC) AS rn FROM Commitment INNER JOIN [Providers] p on p.Ukprn = Commitment.ProviderId) c
		WHERE rn = 1
	) p
	ON a.ProviderId = p.ProviderId
GROUP BY a.ProviderId, p.ProviderName
