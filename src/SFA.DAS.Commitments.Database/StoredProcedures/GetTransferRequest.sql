CREATE PROCEDURE [dbo].GetTransferRequest
	@transferRequestId BIGINT
AS

SELECT 
	TR.Id AS TransferRequestId
	,C.EmployerAccountId AS ReceivingEmployerAccountId
	,ale.[Name] as ReceivingLegalEntityName
	,C.Reference as CohortReference
	,TR.[CommitmentId]
	,C.TransferSenderId AS SendingEmployerAccountId
	,ts.[Name] as 'TransferSenderName'
	,ale.[Name] as 'LegalEntityName'
    ,TR.[Cost] AS TransferCost
	,TR.FundingCap as FundingCap
	,TR.TrainingCourses
    ,TR.[Status]
    ,TR.TransferApprovalActionedByEmployerName AS ApprovedOrRejectedByUserName
    ,TR.TransferApprovalActionedByEmployerEmail AS ApprovedOrRejectedByUserEmail
    ,TR.TransferApprovalActionedOn AS ApprovedOrRejectedOn
FROM [dbo].[TransferRequest] TR
JOIN [dbo].[Commitment] C ON TR.CommitmentId = C.Id
INNER JOIN [dbo].[Accounts] ts on ts.Id = C.TransferSenderId
INNER JOIN [AccountLegalEntities] ale on ale.Id = c.AccountLegalEntityId
WHERE TR.Id = @transferRequestId