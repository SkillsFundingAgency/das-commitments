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
		apps.ApprenticeshipId,
		ROW_NUMBER() OVER (ORDER BY (SELECT 1)) AS [NewPaymentOrder]
	FROM 
		(
		SELECT TOP 1000000
			a.Id AS ApprenticeshipId
		FROM
			Apprenticeship a

		INNER JOIN 
			Commitment c
		ON
			a.CommitmentId = c.Id
		AND
			c.EmployerAccountId = @employerAccountId
		AND
			a.AgreedOn IS NOT NULL
	
		ORDER BY
			CAST(a.AgreedOn AS DATE),
			a.ULN

		) AS apps -- apprenticeship Ids ordered by agreement date, ULN

	) AS npo -- apprenticeship Ids with corresponding new payment order

INNER JOIN
	Apprenticeship a
ON 
	a.id = npo.ApprenticeshipId
