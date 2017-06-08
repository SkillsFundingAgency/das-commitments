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

UPDATE 
	[dbo].[Apprenticeship]
SET 
	TrainingName = TrainingName + ' (Standard)'
WHERE
	TrainingType = 0
AND 
	RIGHT(TrainingName, 10) <> '(Standard)' 



INSERT INTO [dbo].[PriceHistory]
	(ApprenticeshipId,Cost, FromDate)
	SELECT Id, Cost, StartDate FROM [dbo].[Apprenticeship] 
	WHERE PaymentStatus <> 0
	AND Id NOT IN (SELECT ApprenticeshipId FROM [dbo].[PriceHistory])

