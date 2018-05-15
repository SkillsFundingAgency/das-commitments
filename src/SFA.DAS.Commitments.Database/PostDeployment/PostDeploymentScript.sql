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

BEGIN

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


	UPDATE History
	SET ProviderId = c.ProviderId, EmployerAccountId = c.EmployerAccountId
	FROM History h JOIN Commitment c
	ON h.CommitmentId = c.Id

	UPDATE History
	SET ProviderId = a.ProviderId, EmployerAccountId = a.EmployerAccountId
	FROM History h JOIN ApprenticeshipSummary a
	ON h.ApprenticeshipId = a.Id

END

-- This is a One time Action. We only need to import the old tansfer records on upgrade. New records will always add a TransferRequest record (when both parties approve)
IF NOT EXISTS(SELECT * FROM [dbo].[TransferRequest]) 
BEGIN

  	INSERT INTO TransferRequest (CommitmentId, TrainingCourses, Cost, [Status], TransferApprovalActionedByEmployerEmail, TransferApprovalActionedByEmployerName, TransferApprovalActionedOn)
	SELECT Id, '[]', (SELECT SUM(Cost) FROM Apprenticeship a WHERE a.CommitmentId = c.Id),  ISNULL(TransferApprovalStatus, 0), 
		TransferApprovalActionedByEmployerEmail, TransferApprovalActionedByEmployerName, TransferApprovalActionedOn  
	FROM [dbo].[Commitment] c WHERE c.TransferSenderId IS NOT NULL AND c.EditStatus = 0 AND c.CommitmentStatus = 1
	
END

--todo: one-time update to be run when the system quiesces
--UPDATE Apprenticeship
--SET    PendingUpdateOriginator = AU.Originator
--FROM   Apprenticeship A
--INNER JOIN ApprenticeshipUpdate AU
--ON     A.Id = AU.ApprenticeshipId
--	   AND AU.Status = 0;
