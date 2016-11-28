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


UPDATE Commitment SET CreatedOn = '01 Nov 2016' WHERE CreatedOn IS NULL;
UPDATE Apprenticeship SET CreatedOn = '01 Nov 2016' WHERE CreatedOn IS NULL;

ALTER TABLE Commitment ALTER COLUMN CreatedOn DATETIME NOT NULL;
ALTER TABLE Apprenticeship ALTER COLUMN CreatedOn DATETIME NOT NULL;

