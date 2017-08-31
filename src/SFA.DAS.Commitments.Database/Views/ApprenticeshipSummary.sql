CREATE VIEW [dbo].[ApprenticeshipSummary]
AS 

SELECT 
	a.Id,a.CommitmentId,a.FirstName,a.LastName,a.ULN,a.TrainingType,a.TrainingCode,a.TrainingName,
	a.StartDate,a.EndDate,a.AgreementStatus,a.PaymentStatus,a.DateOfBirth,a.NINumber,a.EmployerRef,
	a.ProviderRef,a.CreatedOn,a.AgreedOn,a.PaymentOrder,a.StopDate,
	c.EmployerAccountId, c.ProviderId, c.Reference, c.LegalEntityName, c.ProviderName, c.LegalEntityId,
	au.Originator AS UpdateOriginator,
	CASE WHEN dlPrice.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockPrice',
	CASE WHEN dlPriceTriaged.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockPriceTriaged',
	CASE WHEN dlCourse.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockCourse',
	CASE WHEN dlCourseTriaged.Id IS NULL THEN CAST(0 as bit) ELSE CAST(1 as bit) END 'DataLockCourseTriaged',
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
	LEFT JOIN DataLockStatus dlPrice on dlPrice.Id =
		(
			SELECT TOP 1 
        Id 
      FROM 
        DataLockStatus
			WHERE 
        ApprenticeshipId = a.Id 
      AND 
        ErrorCode = 64 
      AND 
        TriageStatus = 0
			AND
        [Status] = 2 
      AND 
        [IsResolved] = 0
      ORDER BY 
        IlrEffectiveFromDate, Id
		)
	LEFT JOIN DataLockStatus dlPriceTriaged on dlPriceTriaged.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id and ErrorCode = 64 and TriageStatus = 1
		and [Status] = 2 AND [IsResolved] = 0
	)
	LEFT JOIN DataLockStatus dlCourse on dlCourse.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id
		and (ErrorCode & 4 = 4  OR ErrorCode & 8 = 8 OR ErrorCode & 16 = 16 OR ErrorCode & 32 = 32)
		and TriageStatus = 0
		and [Status] = 2 AND [IsResolved] = 0
	)
	LEFT JOIN DataLockStatus dlCourseTriaged on dlCourseTriaged.Id =
	(
		SELECT TOP 1 Id from DataLockStatus
		where ApprenticeshipId = a.Id
		and (ErrorCode & 4 = 4 OR ErrorCode & 8 = 8 OR ErrorCode & 16 = 16 OR ErrorCode & 32 = 32)
		and TriageStatus = 2
		and [Status] = 2 AND [IsResolved] = 0
	)