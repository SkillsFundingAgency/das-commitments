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


--CV-565: Update old records that were created before the capture of AccountLegalEntityId
update c set
c.AccountLegalEntityId = ale.Id
from
Commitment c
join AccountLegalEntities ale on ale.PublicHashedId = c.AccountLegalEntityPublicHashedId
where c.AccountLegalEntityId IS NULL


--Remove leftover procs
DROP PROCEDURE IF EXISTS [dbo].[SetTransferRequestApproval]
DROP PROCEDURE IF EXISTS [dbo].[GetApprovedApprenticeship]
DROP PROCEDURE IF EXISTS [dbo].[SetTransferApproval]
DROP PROCEDURE IF EXISTS [dbo].[StartATransferRequest]
DROP PROCEDURE IF EXISTS [dbo].[SetPaymentOrder]
DROP PROCEDURE IF EXISTS [dbo].[GetActiveApprenticeshipsForProvider]
DROP PROCEDURE IF EXISTS [dbo].[GetActiveApprenticeshipsForEmployer]









