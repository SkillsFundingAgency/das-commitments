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
SELECT -- All Providers that are in the custom priority list and currently have "active" commitments
	ppp.ProviderId, 
	ppp.[PriorityOrder], 
	ppp.EmployerAccountId 
	FROM  
		(
			SELECT 
				ProviderId, 
				EmployerAccountid
			FROM AllProviders a
			INTERSECT
			SELECT 
				ProviderId, 
				EmployerAccountid 
			FROM CustomProviderPaymentPriority
		) m
INNER JOIN 
	CustomProviderPaymentPriority ppp 
ON 
	ppp.Providerid = m.ProviderId
UNION
SELECT -- All providers that have "active" commitments but aren't in the custom priority list
	ap.ProviderId, 
	ap.ProviderOrder + 100, 
	ap.employeraccountid 
	FROM
		(
			SELECT 
				ProviderId, 
				EmployerAccountid
			FROM AllProviders
			EXCEPT
			SELECT 
				ProviderId,
				EmployerAccountid
			FROM CustomProviderPaymentPriority
		) e
INNER JOIN 
	AllProviders ap 
ON 
	e.ProviderId = ap.ProviderId
