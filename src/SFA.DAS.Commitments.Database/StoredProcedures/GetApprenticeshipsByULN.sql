CREATE PROCEDURE [dbo].[GetApprenticeshipsByULN]
(
  @ULN NVARCHAR(50),
  @accountId BIGINT
)
AS
wITH EmployerApprenticeCTE (ULN)
AS
(	SELECT DISTINCT 
		s.ULN
	FROM 
		ApprenticeshipSummary s
	WHERE 
		s.ULN = @ULN
	AND 
		S.EmployerAccountId = @accountId
	)

	SELECT DISTINCT
		   s.[Id]
		  ,s.[CommitmentId]
		  ,s.[FirstName]
		  ,s.[LastName]
		  ,s.[Cost]
		  ,s.[ULN]
		  ,s.[TrainingType]
		  ,s.[TrainingCode]
		  ,s.[TrainingName]
		  ,s.[StartDate]
		  ,s.[EndDate]
		  ,s.[AgreementStatus]
		  ,s.[PaymentStatus]
		  ,s.[DateOfBirth]
		  ,s.[NINumber]
		  ,s.[EmployerRef]
		  ,s.[ProviderRef]
		  ,s.[CreatedOn]
		  ,s.[AgreedOn]
		  ,s.[PaymentOrder]
		  ,s.[StopDate]
		  ,s.[PauseDate]
		  ,s.[HasHadDataLockSuccess]
		  ,s.[EmployerAccountId]
		  ,s.[TransferSenderId]
		  ,s.[ProviderId]
		  ,s.[Reference]
		  ,s.[LegalEntityName]
		  ,s.[ProviderName]
		  ,s.[LegalEntityId]
		  ,s.[AccountLegalEntityPublicHashedId]
		  ,s.[UpdateOriginator]
		  ,s.[DataLockPrice]
		  ,s.[DataLockPriceTriaged]
		  ,s.[DataLockCourse]
		  ,s.[DataLockCourseTriaged]
		  ,s.[DataLockCourseChangeTriaged]
		  ,s.[EmployerCanApproveApprenticeship]
		  ,s.[ProviderCanApproveApprenticeship]
		  ,s.[EndpointAssessorName]
	FROM 
		ApprenticeshipSummary s
	INNER JOIN 
		EmployerApprenticeCTE ea on s.ULN = ea.ULN

SELECT @@ROWCOUNT;