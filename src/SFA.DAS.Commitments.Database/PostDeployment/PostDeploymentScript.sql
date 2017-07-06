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


WITH result (id, jsonApp)
AS
(SELECT id, jsonApp FROM (SELECT * FROM [dbo].[ApprenticeshipSummary]
	WHERE Id in (
		SELECT EntityId FROM [SFA.DAS.Commitments.Database].[dbo].[History]
		WHERE EntityType = 'Apprenticeship'
		AND UpdatedByRole = 'Employer'
		AND ChangeType = 'Created'
		AND UpdatedState like '%providerId":0,%'
		)) a
	CROSS apply
	(
		SELECT * FROM [dbo].[ApprenticeshipSummary] d
		where d.Id = a.Id
		FOR JSON AUTO, WITHOUT_ARRAY_WRAPPER, INCLUDE_NULL_VALUES
	) d(jsonApp)
)

UPDATE [dbo].[History]
SET UpdatedState = c.jsonApp
FROM
[dbo].[History] hi
INNER JOIN
result c
ON c.Id = hi.EntityId
