CREATE VIEW [dbo].[CommitmentSummary]
AS 

SELECT c.*, 
	COUNT(a.Id) AS ApprenticeshipCount,
	COALESCE((SELECT TOP 1 AgreementStatus FROM Apprenticeship WHERE CommitmentId = c.Id), 0) AS AgreementStatus, -- because should all be same value
	CASE
		WHEN COUNT(a.Id) > 0 AND 1 =  (dervx.EmployerCanApproveApprenticeship)
		THEN 1
		ELSE 0 
	END AS EmployerCanApproveCommitment,

	CASE
		WHEN COUNT(a.Id) > 0 AND 1 =  (dervx.ProviderCanApproveApprenticeship) 
		THEN 1 
		ELSE 0 
	END AS ProviderCanApproveCommitment

FROM 
	Commitment c
LEFT JOIN 
	Apprenticeship a ON a.CommitmentId = c.Id
left join
(
	select 
		id, 
		min(aps.EmployerCanApproveApprenticeship) EmployerCanApproveApprenticeship, 
		min(aps.ProviderCanApproveApprenticeship) ProviderCanApproveApprenticeship
	from ApprenticeshipSummary aps 
	group by id) 
dervx on dervx.id = a.id

GROUP BY 
	dervx.EmployerCanApproveApprenticeship,dervx.ProviderCanApproveApprenticeship,
	c.Id, c.Reference, c.EmployerAccountId, c.LegalEntityId,
	c.LegalEntityName, c.ProviderId, c.ProviderName, c.CommitmentStatus, c.EditStatus, 
	c.CreatedOn, c.LastAction, c.LastUpdatedByEmployerEmail, c.LastUpdatedByProviderEmail, 
	c.LastUpdatedByEmployerName, c.LastUpdatedByProviderName, c.LegalEntityAddress, c.LegalEntityOrganisationType