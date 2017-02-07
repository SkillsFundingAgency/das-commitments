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

UPDATE [Commitment] SET [LastUpdatedByEmployerEmail] = 'noreply@sfa.gov.uk', [LastUpdatedByEmployerName] = 'Dummy User' WHERE LastUpdatedByEmployerEmail IS NULL;

-- TODO: This can be moved into the table definition once deployed to all Environments
ALTER TABLE [Commitment] ALTER COLUMN [LastUpdatedByEmployerName] VARCHAR(255) NOT NULL;
ALTER TABLE [Commitment] ALTER COLUMN [LastUpdatedByEmployerEmail] VARCHAR(255) NOT NULL;
