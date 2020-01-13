/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/


--CV-565: Fix missing AccountLegalEntity LegalEntityId values
update c
set c.LegalEntityId = ale.LegalEntityId
from Commitment c
join AccountLegalEntities ale on ale.PublicHashedId = c.AccountLegalEntityPublicHashedId
where c.LegalEntityId = ''

--Update old records that were created before the capture of AccountLegalEntityId
update c set
c.AccountLegalEntityId = ale.Id
from
Commitment c
join AccountLegalEntities ale on ale.PublicHashedId = c.AccountLegalEntityPublicHashedId
where c.AccountLegalEntityId IS NULL
