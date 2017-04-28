CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsByULNs]
(
	@ULNs [ULNTable] READONLY
)
AS

SELECT
a.*,
c.EmployerAccountId,
c.ProviderId,
c.ProviderName,
c.LegalEntityName,
c.Id as 'CommitmentId'
FROM @ULNs u
INNER JOIN [dbo].[Apprenticeship] a ON u.ULN = a.ULN
INNER JOIN [dbo].[Commitment] c on c.Id = a.CommitmentId
WHERE
a.AgreementStatus = 3 -- Both parties agreed - "right of the line"
AND a.PaymentStatus IN (1,2,3) --Active, Paused or Cancelled