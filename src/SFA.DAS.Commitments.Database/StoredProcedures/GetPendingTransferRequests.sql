CREATE PROCEDURE [dbo].[GetPendingTransferRequests]
AS

SELECT 
	TransferRequestId
	,ReceivingEmployerAccountId
	,ReceivingLegalEntityName
	,CohortReference
	,CommitmentId
	,SendingEmployerAccountId
    ,TransferCost
    ,[Status]
    ,ApprovedOrRejectedByUserName
    ,ApprovedOrRejectedByUserEmail
    ,ApprovedOrRejectedOn
	,CreatedOn
FROM [dbo].[TransferRequestSummary]
WHERE [Status] = 0 AND AutoApproval = 0
ORDER BY CommitmentId