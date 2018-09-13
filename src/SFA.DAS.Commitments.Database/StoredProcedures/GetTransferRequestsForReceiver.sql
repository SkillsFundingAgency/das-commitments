CREATE PROCEDURE [dbo].GetTransferRequestsForReceiver
	@receiverEmployerAccountId BIGINT
AS

SELECT 
	TransferRequestId
	,ReceivingEmployerAccountId
	,ReceivingLegalEntityName
	,CohortReference
	,CommitmentId
	,SendingEmployerAccountId
    ,TransferCost
	,FundingCap
    ,[Status]
    ,ApprovedOrRejectedByUserName
    ,ApprovedOrRejectedByUserEmail
    ,ApprovedOrRejectedOn
	,CreatedOn
FROM [dbo].[TransferRequestSummary]
WHERE ReceivingEmployerAccountId = @receiverEmployerAccountId
ORDER BY CommitmentId, CreatedOn