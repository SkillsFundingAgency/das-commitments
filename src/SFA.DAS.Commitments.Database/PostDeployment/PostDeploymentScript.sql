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

IF EXISTS (SELECT COUNT(*) FROM [dbo].[Apprenticeship] WHERE [PaymentStatus] = 2 AND [PauseDate] = null)
BEGIN
	PRINT 'Updating Paused Apprenticeship PauseDate values from History table'
	/*
		Update of [Apprenticeship] using Pause/Resume history records in [History] to 
		Set the [PauseDate] of currently paused items using the latest pause history data [CreatedOn] date
	*/


	UPDATE [dbo].[Apprenticeship] 
	SET	[PauseDate] = CASE 
						WHEN	JSON_VALUE(h.[UpdatedState], '$.PaymentStatus') = '2' THEN h.[CreatedOn]		/* now paused */
						WHEN	JSON_VALUE(h.[UpdatedState], '$.PaymentStatus') = '1' THEN NULL					/* now active */
						ELSE	a.[PauseDate]	/* catch all no change */
						END
	
/*

	SELECT [Id]
		  ,[CommitmentId]
		  ,[StartDate]
		  ,[EndDate]
		  ,[AgreementStatus]
		  ,[PaymentStatus]
		  ,a.[CreatedOn]
		  ,[AgreedOn]
		  ,[PaymentOrder]
		  ,[StopDate]
		  ,[PauseDate]
		  ,h.[CreatedOn]
		  FROM
*/	  
	FROM [dbo].[Apprenticeship] a,
		(	SELECT TOP 1 [EntityId]  ,MAX([CreatedOn]) as [CreatedOn]
			FROM [dbo].[History]
			WHERE	[EntityType]	= 'Apprenticeship'
			AND		[ChangeType]		= 'ChangeOfStatus'
			AND 	(
						JSON_VALUE([OriginalState], '$.PaymentStatus') = '1' AND JSON_VALUE([UpdatedState], '$.PaymentStatus') = '2'
					)
			GROUP BY
				  [EntityId] 
				  ,[CreatedOn]
			ORDER BY
				  [EntityId]  ASC
				  ,[CreatedOn] DESC
		) h
	 WHERE	a.[Id]				= h.[EntityId]
	 AND	a.[PaymentStatus]	= 2
		  

END