CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsByULNs]
(
	@ULNs [ULNTable] READONLY
)
AS

SELECT
a.*
FROM [dbo].[Apprenticeship] a
JOIN @ULNs u ON u.ULN = a.ULN
WHERE
a.AgreementStatus = 3 -- Both parties agreed - "right of the line"
AND a.PaymentStatus IN (1,2) --Active or Paused
