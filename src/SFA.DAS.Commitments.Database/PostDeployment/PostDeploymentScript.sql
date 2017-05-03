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
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='CommitmentHistory' AND TABLE_SCHEMA='dbo')
BEGIN

	INSERT INTO [dbo].[History]
		(EntityType, EntityId, UserId, UpdatedByRole, ChangeType, CreatedOn)
	SELECT 'Commitment',
		   CommitmentId,
		   UserId,
		   CASE UpdatedByRole WHEN 0 THEN 'Employer' ELSE 'Provider' END,
		   CASE ChangeType WHEN 0 THEN 'Created' WHEN 1 THEN 'Deleted' WHEN 2 THEN 'CreatedApprenticeship' WHEN 3 THEN 'DeletedApprenticeship' WHEN 4 THEN 'EditedApprenticeship' WHEN 5 THEN 'SentForReview' WHEN 6 THEN 'SentForApproval' WHEN 7 THEN 'FinalApproval' END,
		   CreatedOn
	FROM [dbo].[CommitmentHistory]

	DROP TABLE [dbo].[CommitmentHistory]
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='ApprenticeshipHistory' AND TABLE_SCHEMA='dbo')
BEGIN

	INSERT INTO [dbo].[History]
		(EntityType, EntityId, UserId, UpdatedByRole, ChangeType, CreatedOn)
	SELECT 'Apprenticeship',
		   ApprenticeshipId,
		   UserId,
		   CASE UpdatedByRole WHEN 0 THEN 'Employer' ELSE 'Provider' END,
		   CASE ChangeType WHEN 0 THEN 'Created' WHEN 1 THEN 'Updated' WHEN 2 THEN 'ChangeOfStatus' WHEN 3 THEN 'ApprovingChange' WHEN 4 THEN 'DataLockFailureOccured' WHEN 5 THEN 'DataLockFailureResolved' END,
		   CreatedOn
	FROM [dbo].[ApprenticeshipHistory]

	DROP TABLE [dbo].[ApprenticeshipHistory]
END