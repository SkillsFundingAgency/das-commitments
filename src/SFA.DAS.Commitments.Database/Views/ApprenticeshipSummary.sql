CREATE VIEW [dbo].[ApprenticeshipSummary]
AS 

SELECT 
	a.Id,a.CommitmentId,a.FirstName,a.LastName,a.ULN,a.TrainingType,a.TrainingCode,a.TrainingName,
	a.StartDate,a.EndDate,a.AgreementStatus,a.PaymentStatus,a.DateOfBirth,a.NINumber,a.EmployerRef,
	a.ProviderRef,a.CreatedOn,a.AgreedOn,a.PaymentOrder,a.StopDate,
	c.EmployerAccountId, c.ProviderId, c.Reference, c.LegalEntityName, c.ProviderName, c.LegalEntityId,
	au.Originator AS UpdateOriginator,
	dl.TriageStatus AS DataLockTriage, dl.ErrorCode as DataLockErrorCode,
	CASE
		WHEN
			a.PaymentStatus = 0

		THEN
			a.Cost
		ELSE
			(
			SELECT TOP 1 Cost
				FROM PriceHistory
				WHERE ApprenticeshipId = a.Id
				AND (
					(FromDate <= GETDATE() AND ToDate >= FORMAT(GETDATE(),'yyyMMdd')) 
					OR ToDate IS NULL
				)
				ORDER BY FromDate
			 )
	END AS 'Cost',
	CASE 
		WHEN
			a.FirstName IS NOT NULL AND 
			a.LastName IS NOT NULL AND 
			a.Cost IS NOT NULL AND 
			a.StartDate IS NOT NULL AND 
			a.EndDate IS NOT NULL AND 
			a.TrainingCode IS NOT NULL AND 
			a.DateOfBirth IS NOT NULL
		THEN
			1
		ELSE
			0
	END AS 'EmployerCanApproveApprenticeship',
	CASE 
		WHEN
			a.FirstName IS NOT NULL AND 
			a.LastName IS NOT NULL AND 
			a.ULN IS NOT NULL AND -- ULN is required for provider approval
			a.Cost IS NOT NULL AND 
			a.StartDate IS NOT NULL AND 
			a.EndDate IS NOT NULL AND 
			a.TrainingCode IS NOT NULL AND 
			a.DateOfBirth IS NOT NULL
		THEN
			1
		ELSE
			0
	END AS 'ProviderCanApproveApprenticeship'

	FROM 
		Apprenticeship a
	INNER JOIN 
		Commitment c
	ON 
		c.Id = a.CommitmentId
	LEFT JOIN
		(SELECT ApprenticeshipId, Originator FROM ApprenticeshipUpdate WHERE Status = 0) AS au 
		ON au.ApprenticeshipId = a.Id
	LEFT JOIN DataLockStatus dl on dl.Id =
		(
			SELECT TOP 1 Id from DataLockStatus
			where ApprenticeshipId = a.Id
			and [Status] = 2 AND [IsResolved] = 0
			and SUBSTRING(PriceEpisodeIdentifier,LEN(PriceEpisodeIdentifier)-9,10) <> '01/08/2017'
			ORDER BY CONVERT(DATETIME,SUBSTRING(PriceEpisodeIdentifier,LEN(PriceEpisodeIdentifier)-9,10),103) ASC 
		)
