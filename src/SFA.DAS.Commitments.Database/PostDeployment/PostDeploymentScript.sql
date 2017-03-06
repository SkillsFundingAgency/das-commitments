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




/*
This script populates the relationship table for existing commitments, as relationships are usually created the first time a
commitment is created for a given Provider-Employer-LegalEntity combination.
*/

insert into [dbo].[Relationship]
select
distinct
c.ProviderId,
c.ProviderName,
c.EmployerAccountId,
c.LegalEntityId,
c.LegalEntityName,
c.LegalEntityAddress,
c.LegalEntityOrganisationType,
null as Verified,
GETDATE() as CreatedOn
from
[dbo].[Commitment] c
where not exists (select 1 from [dbo].[Relationship] r where r.EmployerAccountId = c.EmployerAccountId and r.LegalEntityId = c.LegalEntityId and r.ProviderId = c.ProviderId);


/* Defaults new columns to not null and sets a default */
update [dbo].[Relationship] set LegalEntityAddress = '', LegalEntityOrganisationType=1 where LegalEntityAddress is null;
alter table [dbo].[Relationship] alter column [LegalEntityAddress] nvarchar(256) not null;
alter table [dbo].[Relationship] alter column [LegalEntityOrganisationType] tinyint not null;

update [dbo].[Commitment] set LegalEntityAddress = '', LegalEntityOrganisationType=1 where LegalEntityAddress is null;
alter table [dbo].[Commitment] alter column [LegalEntityAddress] nvarchar(256) not null;
alter table [dbo].[Commitment] alter column [LegalEntityOrganisationType] tinyint not null;


