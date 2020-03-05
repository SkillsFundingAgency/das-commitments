-- gets all appenticeships that have been approved by employer and provider (unless subsequently completed or deleted)
-- if transfer funded, we still return apprenticeships that haven't been approved by the sender yet
CREATE PROCEDURE [dbo].[GetActiveApprenticeshipsByULNs]
(
	@ULNs [ULNTable] READONLY
)
AS

BEGIN
SELECT
a.*,
c.EmployerAccountId,
c.TransferSenderId,
c.ProviderId,
c.ProviderName,
c.LegalEntityName,
c.Id AS 'CommitmentId',
a.PaymentStatus
FROM @ULNs u
INNER JOIN [dbo].[Apprenticeship] a ON u.ULN = a.ULN
INNER JOIN [dbo].[Commitment] c ON c.Id = a.CommitmentId
WHERE
a.Approvals in (3,7)
END