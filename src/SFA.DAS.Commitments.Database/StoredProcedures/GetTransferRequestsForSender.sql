CREATE PROCEDURE [dbo].GetTransferRequestsForSender
	@senderEmployerAccountId BIGINT
AS

SELECT 
	TR.Id AS TransferRequestId
	,C.EmployerAccountId AS ReceivingEmployerAccountId
	,TR.[CommitmentId]
	,C.TransferSenderId AS SendingEmployerAccountId
    ,TR.[Cost] AS TransferCost
    ,TR.[Status] AS TransferApprovalStatus
    ,TR.TransferApprovalActionedByEmployerName AS ApprovedOrRejectedByUserName
    ,TR.TransferApprovalActionedByEmployerEmail AS ApprovedOrRejectedByUserEmail
    ,TR.TransferApprovalActionedOn AS ApprovedOrRejectedOn
FROM [dbo].[TransferRequest] TR
JOIN [dbo].[Commitment] C ON TR.CommitmentId = C.Id
WHERE C.TransferSenderId = @senderEmployerAccountId
ORDER BY TR.CommitmentId, TR.Id