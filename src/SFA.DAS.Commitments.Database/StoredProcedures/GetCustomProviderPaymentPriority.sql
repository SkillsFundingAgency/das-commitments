CREATE PROCEDURE [dbo].[GetCustomProviderPaymentPriority]
	@EmployerAccountId BIGINT
AS

WITH ProviderNames
AS
(
	SELECT 
		ProviderId,
		p.[Name] as 'ProviderName',
		ROW_NUMBER() OVER (PARTITION BY ProviderId ORDER BY CreatedOn DESC) AS RowNumber
	FROM 
		Commitment c
		INNER JOIN [Providers] p on p.Ukprn = c.ProviderId
	WHERE
		c.EmployerAccountId = @EmployerAccountId
)
SELECT  
	ppp.ProviderId,
	n.ProviderName, 
	ROW_NUMBER() OVER (ORDER BY ppp.ProviderOrder) AS PriorityOrder  
FROM 
	ProviderPaymentPriority ppp
INNER JOIN
	ProviderNames n
ON
	n.ProviderId = ppp.ProviderId
WHERE
	n.RowNumber = 1
AND 
	ppp.EmployerAccountId = @EmployerAccountId
ORDER BY
	ppp.ProviderOrder ASC
