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

	DECLARE @skip INT = (@batchNumber - 1) * @batchSize,
			@TotalCount INT;

	SET @TotalCount = (SELECT COUNT(1) FROM [dbo].[GetLearners] (@sinceTime))

	-- We use the totalcount to calculate the total number of batches.
	SET @totalNumberOfBatches = ISNULL( CEILING( (@TotalCount) / CAST(@batchSize AS DECIMAL)), 0);

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
		,[EmployerAccountId]
		,[EmployerName]
	
	FROM [dbo].[GetLearners] (@sinceTime)

	ORDER BY Seq
	OFFSET @skip ROWS
	FETCH NEXT @batchSize ROWS ONLY

END