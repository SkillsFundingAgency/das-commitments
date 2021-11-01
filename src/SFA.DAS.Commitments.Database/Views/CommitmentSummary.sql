CREATE VIEW [dbo].[CommitmentSummary]
AS 

WITH ApprenticeshipApprovalStatus_CTE (Id, CommitmentId, AgreementStatus, EmployerCanApproveApprenticeship, ProviderCanApproveApprenticeship)
AS (SELECT Id, CommitmentId, AgreementStatus,
	CASE 
		WHEN
			a.FirstName IS NOT NULL AND 
			a.LastName IS NOT NULL AND 
			a.Cost IS NOT NULL AND 
			a.StartDate IS NOT NULL AND 
			a.EndDate IS NOT NULL AND 
			a.TrainingCode IS NOT NULL AND 
			a.DateOfBirth IS NOT NULL
		THEN 1 ELSE 0
	END AS 'EmployerCanApproveApprenticeship',
	CASE 
		WHEN
			a.FirstName IS NOT NULL AND 
			a.LastName IS NOT NULL AND 
			a.ULN IS NOT NULL AND -- ULN is required for provider approval
			a.Cost IS NOT NULL AND 
			a.StartDate IS NOT NULL AND 
			a.EndDate IS NOT NULL AND 
			a.TrainingCode IS NOT NULL AND 
			a.DateOfBirth IS NOT NULL
		THEN 1 ELSE 0
	END AS 'ProviderCanApproveApprenticeship'

	FROM 
		Apprenticeship a
)
SELECT
	c.Id, c.Reference, c.EmployerAccountId,
	ale.LegalEntityId as 'LegalEntityId', ale.[Name] as 'LegalEntityName', c.ProviderId, p.[Name] as 'ProviderName',
	c.CommitmentStatus, c.EditStatus, c.CreatedOn, c.LastAction, 
	c.LastUpdatedByEmployerEmail, c.LastUpdatedByProviderEmail, c.LastUpdatedByEmployerName, c.LastUpdatedByProviderName,
	ale.[Address] as 'LegalEntityAddress',
	ale.[OrganisationType] as 'LegalEntityOrganisationType',
	c.TransferSenderId,
	ts.[Name] as 'TransferSenderName',
	c.TransferApprovalStatus, c.TransferApprovalActionedOn, ale.PublicHashedId as 'AccountLegalEntityPublicHashedId', c.Originator, c.ApprenticeshipEmployerTypeOnApproval,
	c.IsFullApprovalProcessed, c.IsDeleted, c.AccountLegalEntityId, c.IsDraft, c.WithParty, c.RowVersion, c.LastUpdatedOn, c.Approvals, c.EmployerAndProviderApprovedOn,
	c.ChangeOfPartyRequestId,
	COUNT(a.Id) AS ApprenticeshipCount,

	COALESCE((SELECT TOP 1 AgreementStatus FROM ApprenticeshipApprovalStatus_CTE WHERE CommitmentId = c.Id), 0) AS AgreementStatus, -- because should all be same value

	CASE
		WHEN COUNT(a.Id) > 0 AND 1 = ALL (SELECT EmployerCanApproveApprenticeship FROM ApprenticeshipApprovalStatus_CTE WHERE CommitmentId = c.Id) 
		THEN 1
		ELSE 0 
	END AS EmployerCanApproveCommitment,

	CASE
		WHEN COUNT(a.Id) > 0 AND 1 = ALL (SELECT ProviderCanApproveApprenticeship FROM ApprenticeshipApprovalStatus_CTE WHERE CommitmentId = c.Id) 
		THEN 1 
		ELSE 0 
	END AS ProviderCanApproveCommitment

FROM Commitment c
LEFT JOIN  Apprenticeship a ON a.CommitmentId = c.Id
INNER JOIN [Providers] p on p.Ukprn = c.ProviderId
INNER JOIN [AccountLegalEntities] ale on ale.Id = c.AccountLegalEntityId
LEFT JOIN [Accounts] ts on ts.Id = c.TransferSenderId
WHERE
c.IsDeleted = 0
GROUP BY 
	c.Id, c.Reference, c.EmployerAccountId,
	ale.LegalEntityId, ale.[Name],
	c.ProviderId, p.[Name],
	c.CommitmentStatus, c.EditStatus, c.CreatedOn, c.LastAction, 
	c.LastUpdatedByEmployerEmail, c.LastUpdatedByProviderEmail, c.LastUpdatedByEmployerName, c.LastUpdatedByProviderName,
	ale.[Address],
	ale.[OrganisationType],
	c.TransferSenderId,
	ts.[Name],
	c.TransferApprovalStatus, c.TransferApprovalActionedOn, ale.PublicHashedId, c.Originator, c.ApprenticeshipEmployerTypeOnApproval,
	c.IsFullApprovalProcessed, c.IsDeleted, c.AccountLegalEntityId, c.IsDraft, c.WithParty, c.RowVersion, c.LastUpdatedOn, c.Approvals, c.EmployerAndProviderApprovedOn,
	c.ChangeOfPartyRequestId