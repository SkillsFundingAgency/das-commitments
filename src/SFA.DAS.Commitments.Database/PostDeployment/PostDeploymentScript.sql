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

EXEC sp_refreshview [dbo.CommitmentSummaryWithMessages]

--CV-515: Add IsDraft flag - update existing data
update Commitment set IsDraft = 1 where LastAction = 0


--CV-516: Add WithParty field - update existing data
update Commitment set WithParty = 
CASE
    WHEN (EditStatus = 1) THEN 1 --Employer
    WHEN (EditStatus = 2) THEN 2 --Provider
    WHEN (EditStatus = 0 AND TransferSenderId is null) THEN 0 --Approved by Employer and Provider and no Transfer Sender
    WHEN (EditStatus = 0 AND TransferApprovalStatus=0) THEN 4 --Approved by Employer and Provider and pending Transfer Sender's approval
    ELSE 0
END