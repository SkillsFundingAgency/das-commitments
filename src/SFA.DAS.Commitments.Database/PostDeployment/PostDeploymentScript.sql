﻿/*
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

IF EXISTS (SELECT * FROM [dbo].[Apprenticeship] WHERE [PaymentStatus] = 2 AND [PauseDate] is NULL)
BEGIN
	PRINT 'Updating Paused Apprenticeship PauseDate values from History table'
	/*
		Update of [Apprenticeship] using Pause/Resume history records in [History] to 
		Set the [PauseDate] of currently paused items using the latest pause history data [CreatedOn] date
	*/


	UPDATE	[dbo].[Apprenticeship] 
	
	SET		[PauseDate]			= h.[CreatedOn]

	FROM	[dbo].[Apprenticeship] a,

		(	SELECT	[EntityId]  ,
					MAX([CreatedOn]) as [CreatedOn]
			
			FROM	[dbo].[History]

			WHERE	[EntityType]	= 'Apprenticeship'
			AND		[ChangeType]		= 'ChangeOfStatus'
			AND 	(
							JSON_VALUE([OriginalState], '$.PaymentStatus') = '1' 
						AND JSON_VALUE([UpdatedState] , '$.PaymentStatus') = '2'
					)
			
			GROUP BY
				  [EntityId] 
		) h

	 WHERE	a.[Id]				= h.[EntityId]
	 AND	a.[PaymentStatus]	= 2		  

END


------ Extended EDIT

-- Marking Apprenticeships as HasHadDataLockSuccess so that they cannot be used in extended edit. 
-- Only if they have a DataLockStatus with ErrirCode Success (0)

UPDATE Apprenticeship 
SET HasHadDataLockSuccess = 1
WHERE Id IN (
  SELECT ApprenticeshipId FROM DataLockStatus
  WHERE ErrorCode = 0
)

--