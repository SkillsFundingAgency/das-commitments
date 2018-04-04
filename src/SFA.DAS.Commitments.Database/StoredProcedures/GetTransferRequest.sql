CREATE PROCEDURE [dbo].GetTransferRequest
	@transferRequestId BIGINT
AS

SELECT 
	TR.Id AS TransferRequestId
	,C.EmployerAccountId AS ReceivingEmployerAccountId
	,TR.[CommitmentId]
	,C.TransferSenderId AS SendingEmployerAccountId
	,C.LegalEntityName
    ,TR.[Cost] AS TransferCost
	,TR.TrainingCourses
    ,TR.[Status] AS TransferApprovalStatus
    ,TR.TransferApprovalActionedByEmployerName AS ApprovedOrRejectedByUserName
    ,TR.TransferApprovalActionedByEmployerEmail AS ApprovedOrRejectedByUserEmail
    ,TR.TransferApprovalActionedOn AS ApprovedOrRejectedOn
FROM [dbo].[TransferRequest] TR
JOIN [dbo].[Commitment] C ON TR.CommitmentId = C.Id
WHERE TR.Id = @transferRequestId