CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsByULNs]
(
	@ULNs [ULNTable] READONLY
)
AS

SELECT
a.*,
c.ProviderId,
c.ProviderName,
c.LegalEntityName
FROM [dbo].[Apprenticeship] a
JOIN [dbo].[Commitment] c on c.Id = a.CommitmentId
JOIN @ULNs u ON u.ULN = a.ULN
WHERE
a.AgreementStatus = 3 -- Both parties agreed - "right of the line"
AND a.PaymentStatus IN (1,2) --Active or Paused
