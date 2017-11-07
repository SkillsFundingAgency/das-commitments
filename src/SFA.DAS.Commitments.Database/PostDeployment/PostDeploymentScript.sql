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

------ Extended EDIT

-- Marking Apprenticeships as HasHadDataLockSuccess so that they cannot be used in extended edit. 
-- Only if they have a DataLockStatus with ErrirCode Success (0)

UPDATE Apprenticeship 
SET HasHadDataLockSuccess = 1
WHERE Id IN (
  SELECT ApprenticeshipId FROM DataLockStatus
  WHERE ErrorCode = 0
)


-- Setting CommitmentId or ApprenticeshipId values on new Columns

UPDATE History
SET CommitmentId = EntityId, 
EntityId = NULL, 
EntityType = NULL
WHERE EntityType = 'Commitment'

UPDATE History
SET ApprenticeshipId = EntityId,
EntityId = NULL,  
EntityType = NULL
WHERE EntityType = 'Apprenticeship'
