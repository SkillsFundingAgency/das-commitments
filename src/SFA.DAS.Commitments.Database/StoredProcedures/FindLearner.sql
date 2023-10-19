CREATE PROCEDURE [dbo].[FindLearner]
	@firstName NVARCHAR(100),
	@lastName NVARCHAR(100),
	@dateOfBirth DATETIME
AS
BEGIN
	SELECT 
		[ApprenticeshipId]
		,[FirstName]
		,[LastName]
		,[DateOfBirth]
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
	FROM [dbo].[GetLearners] (NULL)
	WHERE 
		[FirstName] = @firstName
	AND
		[LastName] = @lastName
	AND
		[DateOfBirth] = @dateOfBirth
END
