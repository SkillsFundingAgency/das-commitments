CREATE PROCEDURE [dbo].[GetLearnersBatch]
	@sinceTime DATETIME,
	@batchNumber INT OUTPUT,
	@batchSize INT OUTPUT,
	@totalNumberOfBatches INT OUTPUT
AS
BEGIN

	IF (ISNULL(@batchNumber, 0) = 0) OR (@batchNumber < 1) 
	BEGIN
		SET @batchNumber = 1;
	END

	IF (ISNULL(@batchSize, 0) = 0) OR (@batchSize < 1) 
	BEGIN
		SET @batchSize = 1000;
	END

	DECLARE @skip INT = (@batchNumber - 1) * @batchSize

	SELECT 
		[ApprenticeshipId]
		,[FirstName]
		,[LastName]
		,[ULN]
		,[TrainingCode]
		,[TrainingCourseVersion]
		,[TrainingCourseVersionConfirmed]
		,[TrainingCourseOption]
		,[StandardUId]
		,[StartDate]
		,[EndDate]
		,[CreatedOn]
		,[UpdatedOn]
		,[StopDate]
		,[PauseDate]
		,[CompletionDate]
		,[UKPRN]
		,[LearnRefNumber]
		,[PaymentStatus]
		,TotalCount
	
	INTO
		#Results

	FROM
		[dbo].[GetLearners] (@sinceTime)

	-- We use the totalcount to calculate the total number of batches.
	SELECT @totalNumberOfBatches = ISNULL( CEILING( (SELECT MAX(TotalCount) FROM #Results) / CAST(@batchSize AS DECIMAL)), 0);

	SELECT 
		[ApprenticeshipId]
		,[FirstName]
		,[LastName]
		,[ULN]
		,[TrainingCode]
		,[TrainingCourseVersion]
		,[TrainingCourseVersionConfirmed]
		,[TrainingCourseOption]
		,[StandardUId]
		,[StartDate]
		,[EndDate]
		,[CreatedOn]
		,[UpdatedOn]
		,[StopDate]
		,[PauseDate]
		,[CompletionDate]
		,[UKPRN]
		,[LearnRefNumber]
		,[PaymentStatus]
	
	FROM
		#Results

	ORDER BY ISNULL(UpdatedOn,CreatedOn)
	OFFSET @skip ROWS
	FETCH NEXT @batchSize ROWS ONLY

END