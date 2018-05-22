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

--one-time update to set the new PendingUpdateOriginator column on Apprenticeship from the ApprenticeshipUpdateColumn
--todo: needs to be removes once has been deployed to production
UPDATE Apprenticeship
SET    PendingUpdateOriginator = AU.Originator
FROM   Apprenticeship A
INNER JOIN ApprenticeshipUpdate AU
ON     A.Id = AU.ApprenticeshipId
	   AND AU.Status = 0;
