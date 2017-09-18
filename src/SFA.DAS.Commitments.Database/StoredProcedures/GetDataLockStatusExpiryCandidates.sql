CREATE PROCEDURE [dbo].[GetDataLockStatusExpiryCandidates]
	@BeforeDate DATETIME ,
	@ExpirableErrorCodes int
	
	WITH RECOMPILE
AS

	SELECT 
		[Id] ,
		[DataLockEventId],
		[DataLockEventDatetime],
		[PriceEpisodeIdentifier],
		[ApprenticeshipId],
		[IlrTrainingCourseCode],
		[IlrTrainingType] ,
		[IlrActualStartDate],
		[IlrEffectiveFromDate] ,
		[IlrTotalCost] ,
		[ErrorCode],
		[Status] ,
		[TriageStatus],
		[ApprenticeshipUpdateId],
		[IsResolved],
		[IsExpired],
		[Expired] 
	FROM 
		[dbo].[DataLockStatus]
	WHERE 
		[IlrEffectiveFromDate] < @BeforeDate
	AND
		[IsExpired] = 0
	AND
		(	(([ErrorCode] & 1   =  1)   AND (@ExpirableErrorCodes & 1   = 1))
		OR	(([ErrorCode] & 2   =  2)   AND (@ExpirableErrorCodes & 2   = 2))
		OR	(([ErrorCode] & 4   =  4)   AND (@ExpirableErrorCodes & 4   = 4))
		OR	(([ErrorCode] & 8   =  8)   AND (@ExpirableErrorCodes & 8   = 8))
		OR	(([ErrorCode] & 16  =  16)  AND (@ExpirableErrorCodes & 16  = 16))
		OR	(([ErrorCode] & 32  =  32)  AND (@ExpirableErrorCodes & 32  = 32))
		OR	(([ErrorCode] & 64  =  64)  AND (@ExpirableErrorCodes & 64  = 64))
		OR	(([ErrorCode] & 128 =  128) AND (@ExpirableErrorCodes & 128 = 128))
		OR	(([ErrorCode] & 512 =  512) AND (@ExpirableErrorCodes & 512 = 512))
		)

