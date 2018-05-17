CREATE PROCEDURE [dbo].[GetPendingTransferRequests]
AS

SELECT 
	TransferRequestId
	,ReceivingEmployerAccountId
	,ReceivingLegalEntityName
	,CommitmentId
	,SendingEmployerAccountId
    ,TransferCost
    ,[Status]
    ,ApprovedOrRejectedByUserName
    ,ApprovedOrRejectedByUserEmail
    ,ApprovedOrRejectedOn
	,CreatedOn
FROM [dbo].[TransferRequestSummary]
WHERE [Status] = 0
ORDER BY CommitmentId, CreatedOn