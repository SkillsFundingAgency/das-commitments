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

UPDATE [Commitment] SET [LastUpdatedByEmployerEmail] = 'noreply@sfa.gov.uk' WHERE LastUpdatedByEmployerEmail IS NULL;

ALTER TABLE [Commitment] ALTER COLUMN [LastUpdatedByEmployerEmail] VARCHAR(255) NOT NULL;
