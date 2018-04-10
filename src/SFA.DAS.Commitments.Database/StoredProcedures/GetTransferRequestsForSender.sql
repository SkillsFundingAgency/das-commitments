CREATE PROCEDURE [dbo].GetTransferRequestsForSender
	@senderEmployerAccountId BIGINT
AS

SELECT 
	TransferRequestId
	,ReceivingEmployerAccountId
	,CommitmentId
	,SendingEmployerAccountId
    ,TransferCost
    ,[Status]
    ,ApprovedOrRejectedByUserName
    ,ApprovedOrRejectedByUserEmail
    ,ApprovedOrRejectedOn
	,CreatedOn
FROM [dbo].[TransferRequestSummary]
WHERE SendingEmployerAccountId = @senderEmployerAccountId
ORDER BY CommitmentId, CreatedOn