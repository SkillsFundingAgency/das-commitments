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
		,ap1.[FirstName]
		,ap1.[LastName]
		,ap1.[DateOfBirth]
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
		,cm1.[EmployerAccountId]
		,ale.[Name] AS [EmployerName]
		,ROW_NUMBER() OVER (ORDER BY [LastUpdated], ap1.Id ) Seq        

	FROM [dbo].[Apprenticeship] ap1
		JOIN [dbo].[Commitment] cm1 ON cm1.Id = ap1.CommitmentId
		INNER JOIN  [dbo].AccountLegalEntities ale ON ale.Id = cm1.AccountLegalEntityId

	WHERE [TrainingType] = 0
	  AND [TrainingCode] is not null
	  AND ISNUMERIC(TrainingCode) = 1
	  AND [StartDate] is not null  
	  AND [EndDate] is not null  
	  AND [ULN] is not null  
	  AND ([LastUpdated] > @sinceTime OR @sinceTime IS NULL)
)

