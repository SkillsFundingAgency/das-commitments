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

-- Task templates
SET IDENTITY_INSERT  [dbo].[TaskTemplates] ON 
IF (NOT EXISTS(SELECT * FROM [dbo].[TaskTemplates] WHERE Id = 1 AND Name = 'Example task'))
	BEGIN 
		INSERT INTO [dbo].[TaskTemplates](Id, Name) 
		VALUES(1, 'Example task') 
	END 
ELSE 
	BEGIN 
		UPDATE [dbo].[TaskTemplates] 
		SET Name = 'Example task'
		WHERE Id = 1
	END 

-- etc.