CREATE FUNCTION [dbo].[GetLearners]
(	
	@sinceTime DATETIME
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
		ap1.Id [ApprenticeshipId]
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
		,ap1.[CreatedOn]
		,[UpdatedOn]
		,[StopDate]
		,[PauseDate]
		,[CompletionDate]
		,ProviderId [UKPRN]
		,ProviderRef LearnRefNumber
		,[PaymentStatus]
		,TotalCount = COUNT(1) OVER()

	FROM [dbo].[Apprenticeship] ap1
		JOIN [dbo].[Commitment] cm1 ON cm1.Id = ap1.CommitmentId

	WHERE [TrainingType] = 0
	  AND [TrainingCode] is not null
	  and ISNUMERIC(TrainingCode) = 1
	  AND [StartDate] is not null  
	  AND [EndDate] is not null  
	  AND [ULN] is not null  
	  AND (ISNULL(ap1.UpdatedOn,ap1.CreatedOn) > @sinceTime OR @sinceTime IS NULL)
)

