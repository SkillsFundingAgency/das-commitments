CREATE PROCEDURE [dbo].[GetAlertsSummary]
AS

SELECT 
	  LegalEntityId
	, LegalEntityName
	, EmployerAccountId
	, COUNT(*) as TotalCount
	, COUNT(case UpdateOriginator when 1 then 1 else null end) as ChangeOfCircCount
	, COUNT(case DataLockTriage when 2 then 1 else null end) as RestartRequestCount
	FROM (SELECT [Id]
				, [PaymentStatus]
				, [LegalEntityId]
				, [UpdateOriginator]
				, [DataLockTriage]
				, EmployerAccountId
				, LegalEntityName

			FROM [dbo].[ApprenticeshipSummary]
			WHERE PaymentStatus > 0
			AND (DataLockTriage = 2 OR UpdateOriginator = 1)
		) a
GROUP BY LegalEntityId, LegalEntityName, EmployerAccountId
ORDER BY EmployerAccountId