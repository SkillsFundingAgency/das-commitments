/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

INSERT INTO [dbo].[PriceHistory]
	(ApprenticeshipId,Cost, FromDate)
	SELECT Id, Cost, StartDate FROM [dbo].[Apprenticeship] 
	WHERE PaymentStatus <> 0
	AND Id NOT IN (SELECT ApprenticeshipId FROM [dbo].[PriceHistory])

	DECLARE @historyWithMissingIds table(
	EntityId bigint,
	UpdatedState varchar(MAX)
)

INSERT INTO @historyWithMissingIds
SELECT EntityId, UpdatedState FROM [dbo].[History]
		WHERE EntityType = 'Apprenticeship'
		AND UpdatedByRole = 'Employer'
		AND ChangeType = 'Created'
		AND UpdatedState like '%providerId":0,%' 

DECLARE @jsons VARCHAR(MAX) 
SELECT @jsons = COALESCE(@jsons + ', ', '') + a.UpdatedState
FROM @historyWithMissingIds a

DECLARE @t_temp table(
	  Id bigint,
	  CommitmentId bigint,
	  EmployerAccountId bigint, 
	  ProviderId bigint,
	  Reference NVARCHAR(50),
	  [FirstName] NVARCHAR(100), 
	  [LastName] NVARCHAR(100),
	  [DateOfBirth] DATETIME,
	  [NINumber] NVARCHAR(10),
	  [ULN] NVARCHAR(50),
	  [TrainingType] INT, 
	  [TrainingCode] NVARCHAR(20), 
	  [TrainingName] NVARCHAR(126),
	  [Cost] DECIMAL,
	  [StartDate] DATETIME,
	  [EndDate] DATETIME,
	  [PaymentStatus] SMALLINT NOT NULL DEFAULT 0,
	  [AgreementStatus] SMALLINT NOT NULL DEFAULT 0,
	  [EmployerRef] NVARCHAR(50) NULL, 
	  [ProviderRef] NVARCHAR(50) NULL, 
	  [EmployerCanApproveApprenticeship] bit,
	  [ProviderCanApproveApprenticeship] bit,
	  [CreatedOn] NVARCHAR(50) NULL, 
      [AgreedOn] DATETIME NULL, 
      [PaymentOrder] INT NULL,
	  [UpdateOriginator] smallint NULL,
	  [ProviderName] NVARCHAR(50) NULL,
	  [LegalEntityId] bigint null,
	  [LegalEntityName] NVARCHAR(100) NULL,
	  [DataLockPrice] bit,
	  [DataLockPriceTriaged] bit,
	  [DataLockCourse] bit,
	  [DataLockCourseTriaged] bit
)

INSERT INTO @t_temp
SELECT * FROM  
 OPENJSON ( '[' + @jsons + ']' )
	 WITH (
	  Id bigint,
	  CommitmentId bigint,
	  EmployerAccountId bigint,
	  ProviderId bigint,
	  Reference NVARCHAR(50),
	  [FirstName] NVARCHAR(100), 
	  [LastName] NVARCHAR(100),
	  [DateOfBirth] DATETIME,
	  [NINumber] NVARCHAR(10),
	  [ULN] NVARCHAR(50),
	  [TrainingType] INT, 
	  [TrainingCode] NVARCHAR(20), 
	  [TrainingName] NVARCHAR(126),
	  [Cost] DECIMAL,
	  [StartDate] DATETIME,
	  [EndDate] DATETIME,
	  [PaymentStatus] SMALLINT, 
	  [AgreementStatus] SMALLINT, 
	  [EmployerRef] NVARCHAR(50), 
      [ProviderRef] NVARCHAR(50), 
	  [EmployerCanApproveApprenticeship] bit,
	  [ProviderCanApproveApprenticeship] bit,
	  [CreatedOn] NVARCHAR(50),
      [AgreedOn] DATETIME,
      [PaymentOrder] INT,
	  [UpdateOriginator] smallint,
	  [ProviderName] NVARCHAR(50),
	  [LegalEntityId] bigint,
	  [LegalEntityName] NVARCHAR(100),
	  [DataLockPrice] bit,
	  [DataLockPriceTriaged] bit,
	  [DataLockCourse] bit,
	  [DataLockCourseTriaged] bit
	 ) n

UPDATE @t_temp
SET EmployerAccountId = c.EmployerAccountId,
ProviderId = c.ProviderId
FROM @t_temp t
LEFT JOIN
[dbo].[Commitment] c
ON t.CommitmentId = c.Id
WHERE c.EmployerAccountId IS NOT NULL


DECLARE @result table (EntityId bigint, Json varchar(MAX))

INSERT INTO @result
SELECT h.EntityId, json FROM  @historyWithMissingIds h
CROSS APPLY (
	SELECT * FROM @t_temp t
	WHERE h.EntityId = t.Id
	FOR JSON AUTO, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
) t(json)


UPDATE [dbo].[History]
SET UpdatedState = r.Json
FROM @result r
LEFT JOIN
[dbo].[History] h
ON h.EntityId = r.EntityId
