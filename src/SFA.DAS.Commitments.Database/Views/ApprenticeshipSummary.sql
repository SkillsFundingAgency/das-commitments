CREATE VIEW [dbo].[ApprenticeshipSummary]
AS 

SELECT 
	a.Id,a.CommitmentId,a.FirstName,a.LastName, a.Cost,a.ULN,a.TrainingType,a.TrainingCode,a.TrainingName,
	a.StartDate,a.EndDate,	
	CASE
		WHEN c.Approvals & 3 = 3 THEN 3  --Both agreed
		WHEN c.Approvals & 1 = 1 THEN 1  -- Employer only agreed
		WHEN c.Approvals & 2 = 2 THEN 2  -- Provider agreed,
		ELSE 0
	END as AgreementStatus,
	a.PaymentStatus,a.DateOfBirth,a.NINumber,a.EmployerRef,
	a.ProviderRef,a.CreatedOn,a.AgreedOn,a.PaymentOrder,a.StopDate, a.PauseDate, a.CompletionDate, a.HasHadDataLockSuccess,
	a.ContinuationOfId,
	c.EmployerAccountId, c.TransferSenderId, c.ProviderId, c.Reference,
	p.[Name] as 'ProviderName',
	ale.[Name] as 'LegalEntityName',
	ale.[LegalEntityId] as 'LegalEntityId',
	ale.[PublicHashedId] as 'AccountLegalEntityPublicHashedId',
	PendingUpdateOriginator AS UpdateOriginator,
	CASE WHEN dlPrice.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockPrice',
	CASE WHEN dlPriceTriaged.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockPriceTriaged',
	CASE WHEN dlCourse.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockCourse',
	CASE WHEN dlCourseTriaged.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockCourseTriaged',
	CASE WHEN dlCourseChangeTriaged.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockCourseChangeTriaged',
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
	END AS 'ProviderCanApproveApprenticeship',
	ao.Name AS 'EndpointAssessorName',
	a.ReservationId,
	a.OriginalStartDate,
	a.MadeRedundant,
	CASE 
		WHEN
			changeOfPartyRequest.Id IS NOT NULL AND
			changeOfPartyRequest.ChangeOfPartyType =1
		THEN
		    1
		 ELSE
		     0
	END As 'IsEmployerContinuation'
	FROM 
		Apprenticeship a
	INNER JOIN 
		Commitment c
	ON 
		c.Id = a.CommitmentId
	INNER JOIN [Providers] p on p.Ukprn = c.ProviderId
	INNER JOIN [AccountLegalEntities] ale on ale.Id = c.AccountLegalEntityId
	LEFT JOIN
		AssessmentOrganisation ao
	ON
		ao.EPAOrgId = a.EPAOrgId
	LEFT JOIN DataLockStatus dlPrice on dlPrice.Id =
		(
			SELECT TOP 1
        Id 
      FROM 
        DataLockStatus
			WHERE ApprenticeshipId = a.Id
			AND ErrorCode = 64
			AND TriageStatus = 0
			AND [Status] = 2
			AND [IsResolved] = 0
			AND [EventStatus] <> 3
			AND [IsExpired] = 0
      ORDER BY 
        IlrEffectiveFromDate, Id
		)
	LEFT JOIN DataLockStatus dlPriceTriaged on dlPriceTriaged.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id and ErrorCode = 64 and TriageStatus = 1
		and [Status] = 2
		AND [IsResolved] = 0
		AND [EventStatus] <> 3
		AND [IsExpired] = 0
	)
	LEFT JOIN DataLockStatus dlCourse on dlCourse.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id
		and (ErrorCode & 4 = 4  OR ErrorCode & 8 = 8 OR ErrorCode & 16 = 16 OR ErrorCode & 32 = 32)
		and TriageStatus = 0
		and [Status] = 2
		AND [IsResolved] = 0
		AND [EventStatus] <> 3
		AND [IsExpired] = 0
	)
	LEFT JOIN DataLockStatus dlCourseTriaged on dlCourseTriaged.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id
		and (ErrorCode & 4 = 4 OR ErrorCode & 8 = 8 OR ErrorCode & 16 = 16 OR ErrorCode & 32 = 32)
		and TriageStatus = 2
		and [Status] = 2 AND [IsResolved] = 0
		AND [EventStatus] <> 3
		AND [IsExpired] = 0
	)
	LEFT JOIN DataLockStatus dlCourseChangeTriaged on dlCourseChangeTriaged.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id
		and (ErrorCode & 4 = 4 OR ErrorCode & 8 = 8 OR ErrorCode & 16 = 16 OR ErrorCode & 32 = 32)
		and TriageStatus = 1
		and [Status] = 2 AND [IsResolved] = 0
		AND [EventStatus] <> 3
		AND [IsExpired] = 0
	)
	LEFT JOIN ChangeOfPartyRequest changeOfPartyRequest on changeOfPartyRequest.Id =
	(
		SELECT TOP 1 Id from ChangeOfPartyRequest
		where ApprenticeshipId = a.ContinuationOfId
		and  NewApprenticeshipId = a.Id
		and [Status] = 3
	)