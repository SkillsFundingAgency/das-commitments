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

-- Setting ApprenticeshipUpdate (CoC) to deleted if 
-- From DataLock origin and Satus is pending
-- The DataLock should still be marked as a TriageChange and be picked by Employer.
UPDATE dbo.ApprenticeshipUpdate
SET Status = 3 -- Delete
WHERE UpdateOrigin = 2 -- DataLock
AND Status = 0 -- Pending

-- Can remove ApprenticeshipUpdateId if we want.
UPDATE dbo.DataLockStatus
SET ApprenticeshipUpdateId = NULL
WHERE ApprenticeshipUpdateId in 
	(SELECT Id FROM dbo.ApprenticeshipUpdate
		WHERE UpdateOrigin = 2 -- DataLock
		AND Status = 0) -- Pending