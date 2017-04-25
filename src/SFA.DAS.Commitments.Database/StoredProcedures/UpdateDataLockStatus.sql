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

	IF EXISTS(select 1 from [dbo].[DataLockStatus] where ApprenticeshipId = @ApprenticeshipId)
	BEGIN

		update [dbo].[DataLockStatus] set
		DataLockEventId = @DataLockEventId,
		DataLockEventDatetime = @DataLockEventDatetime,
		PriceEpisodeIdentifier = @PriceEpisodeIdentifier,
		IlrTrainingCourseCode = @IlrTrainingCourseCode,
		IlrTrainingType = @IlrTrainingType,
		IlrActualStartDate = @IlrActualStartDate,
		IlrEffectiveFromDate = @IlrEffectiveFromDate,
		IlrTotalCost = @IlrTotalCost,
		ErrorCodes = @ErrorCodes,
		[Status] = @Status,
		TriageStatus = @TriageStatus
		where ApprenticeshipId = @ApprenticeshipId

	END
	ELSE
	BEGIN
	
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

	END
