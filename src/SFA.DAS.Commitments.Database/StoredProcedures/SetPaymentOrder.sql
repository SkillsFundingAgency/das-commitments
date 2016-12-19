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
		SELECT TOP 100000
			a.Id AS ApprenticeshipId,
			ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [NewPaymentOrder]
		FROM (
			SELECT TOP 1000000
				ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [ProviderOrder],
				c.ProviderId, 
				MIN(a.AgreedOn) AS EarliestAgreedOn
			FROM 
				Apprenticeship a
			INNER JOIN 
				Commitment c
			ON 
				c.Id = a.CommitmentId
			WHERE 
				c.EmployerAccountId = @employerAccountId
			AND
				a.AgreementStatus = 3 
			AND 
				a.AgreedOn IS NOT NULL
			GROUP BY 
				c.ProviderId
			ORDER BY
				MIN(a.AgreedOn)
			) op -- EA's providers ordered by earliest agreed apprentice across all the EA's commitments

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
			a.AgreedOn,
			a.ULN
	) AS npo -- apprenticeship Ids with corresponding new payment order (if appropriate) based on provider, agreement date, ULN

INNER JOIN
	Apprenticeship a
ON 
	a.id = npo.ApprenticeshipId
