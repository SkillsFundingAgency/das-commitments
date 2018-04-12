CREATE PROCEDURE [dbo].GetTransferRequestsForReceiver
	@receiverEmployerAccountId BIGINT
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
FROM [dbo].[TransferRequestSummary]
WHERE ReceivingEmployerAccountId = @receiverEmployerAccountId
ORDER BY CommitmentId, TransferRequestId