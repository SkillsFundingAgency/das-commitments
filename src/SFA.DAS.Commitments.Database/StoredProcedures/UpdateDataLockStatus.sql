CREATE PROCEDURE [dbo].[UpdateDataLockStatus]
(
	@DataLockEventId BIGINT,
	@DataLockEventDatetime DATETIME,
    @PriceEpisodeIdentifier NVARCHAR(25),
    @ApprenticeshipId BIGINT,
    @IlrTrainingCourseCode NVARCHAR(20),
    @IlrTrainingType TINYINT,
	@IlrActualStartDate DATETIME,
	@IlrEffectiveFromDate DATETIME,
	@IlrTotalCost DECIMAL,
    @ErrorCodes INT,
    @Status TINYINT,
    @TriageStatus TINYINT
)
AS

-- create or insert (or merge?) based on ApprenticeshipId

	insert into [dbo].[DataLockStatus]
	(
		DataLockEventId,
		DataLockEventDatetime,
		PriceEpisodeIdentifier,
		ApprenticeshipId,
		IlrTrainingCourseCode,
		IlrTrainingType,
		IlrActualStartDate,
		IlrEffectiveFromDate,
		IlrTotalCost,
		ErrorCodes,
		[Status],
		TriageStatus
	)
	values
	(
		@DataLockEventId,
		@DataLockEventDatetime,
		@PriceEpisodeIdentifier,
		@ApprenticeshipId,
		@IlrTrainingCourseCode,
		@IlrTrainingType,
		@IlrActualStartDate,
		@IlrEffectiveFromDate,
		@IlrTotalCost,
		@ErrorCodes,
		@Status,
		@TriageStatus
	)
