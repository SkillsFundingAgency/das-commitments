CREATE VIEW [dbo].[ProviderPaymentPriority]
AS 

	WITH AllProviders
AS
(
	SELECT -- All providers that have "active" commitments
		ROW_NUMBER() OVER (PARTITION BY c.EmployerAccountId ORDER BY MIN(a.AgreedOn)) AS [ProviderOrder],
		c.ProviderId,
		c.EmployerAccountId as [EmployerAccountId],
		MIN(a.AgreedOn) AS [AgreedDate]
	FROM 
		Apprenticeship a
	INNER JOIN 
		Commitment c
	ON 
		c.Id = a.CommitmentId
	WHERE 
		a.AgreedOn IS NOT NULL
	AND
		a.PaymentStatus IN (1, 2) -- Active or Paused
	GROUP BY 
		c.ProviderId,
		c.EmployerAccountId
)
SELECT 
	COALESCE(a1.ProviderId, a2.ProviderId) AS ProviderId,
	COALESCE(cp.PriorityOrder, a2.ProviderOrder + 100) AS ProviderOrder,
	a2.EmployerAccountId
FROM AllProviders a1
INNER JOIN 
	CustomProviderPaymentPriority cp
ON 
	a1.providerid = cp.providerid AND a1.employeraccountid = cp.employeraccountid
RIGHT JOIN 
	AllProviders a2 
ON 
	a1.providerid = a2.providerid AND a1.employeraccountid = a2.employeraccountid
