CREATE PROCEDURE [dbo].[SetPaymentOrder]
	@employerAccountId INT
AS

-- calculate the payment order for all approved apprentices related to the specified employer account.
-- NOTE: does not update the payment order for any apprentices that are not approved

UPDATE Apprenticeship
SET
	PaymentOrder = npo.NewPaymentOrder
FROM
    (
		SELECT TOP 1000000
			a.Id AS ApprenticeshipId,
			ROW_NUMBER() OVER (ORDER BY op.ProviderOrder, CAST(a.AgreedOn AS DATE), a.ULN) AS [NewPaymentOrder]
		FROM (
			SELECT TOP 1000000
				ROW_NUMBER() OVER (ORDER BY MIN(ppp.ProviderOrder)) AS [ProviderOrder],
				c.ProviderId, 
				MIN(a.AgreedOn) AS EarliestAgreedOn
			FROM 
				Apprenticeship a
			INNER JOIN 
				Commitment c
			ON 
				c.Id = a.CommitmentId
			INNER JOIN
				ProviderPaymentPriority ppp
			ON
				ppp.ProviderId = c.ProviderId
			WHERE 
				c.EmployerAccountId = @employerAccountId
			AND 
				a.AgreedOn IS NOT NULL
			AND
				a.PaymentStatus IN (1, 2) -- Active or Paused
			GROUP BY 
				c.ProviderId
			) op -- EA's providers ordered by their custom selection or earliest agreed apprentice across all the EA's commitments

		INNER JOIN 
			Commitment c
		ON 
			c.ProviderId = op.ProviderId
		AND
			c.EmployerAccountId = @employerAccountId

		INNER JOIN 
			Apprenticeship a
		ON
			a.CommitmentId = c.Id

		WHERE
			a.AgreedOn IS NOT NULL
	
		ORDER BY
			op.ProviderOrder,
			CAST(a.AgreedOn AS DATE),
			a.ULN
	) AS npo -- apprenticeship Ids with corresponding new payment order (if appropriate) based on provider, agreement date, ULN

INNER JOIN
	Apprenticeship a
ON 
	a.Id = npo.ApprenticeshipId
