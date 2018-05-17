CREATE VIEW [dbo].[TransferRequestSummary]
AS 

SELECT 
	TR.Id AS TransferRequestId
	,C.EmployerAccountId AS ReceivingEmployerAccountId
	,C.LegalEntityName as ReceivingLegalEntityName
	,TR.[CommitmentId]
	,C.TransferSenderId AS SendingEmployerAccountId
    ,TR.[Cost] AS TransferCost
    ,TR.[Status]
    ,TR.TransferApprovalActionedByEmployerName AS ApprovedOrRejectedByUserName
    ,TR.TransferApprovalActionedByEmployerEmail AS ApprovedOrRejectedByUserEmail
    ,TR.TransferApprovalActionedOn AS ApprovedOrRejectedOn
	,TR.CreatedOn
FROM [dbo].[TransferRequest] TR
JOIN [dbo].[Commitment] C ON TR.CommitmentId = C.Id