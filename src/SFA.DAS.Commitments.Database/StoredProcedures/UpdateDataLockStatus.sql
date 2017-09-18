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
	@IlrPriceEffectiveToDate DATETIME,
	@IlrTotalCost DECIMAL,
    @ErrorCode INT,
    @Status TINYINT,
    @TriageStatus TINYINT,
	@ApprenticeshipUpdateId BIGINT,
	@IsResolved BIT,
	@EventStatus TINYINT
	
)
AS

	IF EXISTS(
		select 1 from [dbo].[DataLockStatus]
		where ApprenticeshipId = @ApprenticeshipId
		and PriceEpisodeIdentifier = @PriceEpisodeIdentifier
	)
	BEGIN

		update [dbo].[DataLockStatus] set
		DataLockEventId = @DataLockEventId,
		DataLockEventDatetime = @DataLockEventDatetime,
		PriceEpisodeIdentifier = @PriceEpisodeIdentifier,
		IlrTrainingCourseCode = @IlrTrainingCourseCode,
		IlrTrainingType = @IlrTrainingType,
		IlrActualStartDate = @IlrActualStartDate,
		IlrEffectiveFromDate = @IlrEffectiveFromDate,
		IlrPriceEffectiveToDate = @IlrPriceEffectiveToDate,
		IlrTotalCost = @IlrTotalCost,
		ErrorCode = @ErrorCode,
		[Status] = @Status,
		TriageStatus = @TriageStatus,
		ApprenticeshipUpdateId = @ApprenticeshipUpdateId,
		IsResolved = @IsResolved,
		EventStatus = @EventStatus
		where
		ApprenticeshipId = @ApprenticeshipId
		and PriceEpisodeIdentifier = @PriceEpisodeIdentifier

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
			IlrPriceEffectiveToDate,
			IlrTotalCost,
			ErrorCode,
			[Status],
			TriageStatus,
			ApprenticeshipUpdateId,
			IsResolved,
			EventStatus
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
			@IlrPriceEffectiveToDate,
			@IlrTotalCost,
			@ErrorCode,
			@Status,
			@TriageStatus,
			@ApprenticeshipUpdateId,
			@IsResolved,
			@EventStatus
		)

	END
