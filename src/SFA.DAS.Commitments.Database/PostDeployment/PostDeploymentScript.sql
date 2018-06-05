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

--after deployment, we need to check that the update didn't miss any in-flight apprenticeshipupdate changes
--this query should return 0 rows. if it doesn't, then the returned rows will need updating, either manually, or by running the update above again (rinse and repeat)

--select a.Id, a.PendingUpdateOriginator
--from apprenticeship a
--inner join apprenticeshipupdate au
--on a.Id = au.ApprenticeshipId
--where (au.Status = 0 and (a.PendingUpdateOriginator <> au.Originator or a.PendingUpdateOriginator is null))
--or (au.Status <> 0 and a.PendingUpdateOriginator is not null)
