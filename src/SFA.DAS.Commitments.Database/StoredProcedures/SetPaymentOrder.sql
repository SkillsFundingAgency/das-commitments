CREATE PROCEDURE [dbo].[SetPaymentOrder]
	@employerAccountId INT
AS

---- calculate the payment order for all approved apprentices related to the specified employer account.
---- NOTE: does not update the payment order for any apprentices that are not approved

UPDATE Apprenticeship
SET
	PaymentOrder = npo.NewPaymentOrder
FROM
    (
		SELECT TOP 1000000
			a.Id AS ApprenticeshipId,
			ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [NewPaymentOrder]
		FROM
			Apprenticeship a

		INNER JOIN 
			Commitment c
		ON
			a.CommitmentId = c.Id

		WHERE
			c.EmployerAccountId = @employerAccountId
		AND
			a.AgreedOn IS NOT NULL
	
		ORDER BY
			a.AgreedOn,
			a.ULN
	) AS npo -- apprenticeship Ids with corresponding new payment order (if appropriate) based on agreement date, ULN

INNER JOIN
	Apprenticeship a
ON 
	a.id = npo.ApprenticeshipId
