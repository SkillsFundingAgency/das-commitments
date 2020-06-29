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
p.[Name] as 'ProviderName',
ale.[Name] as 'LegalEntityName',
c.Id AS 'CommitmentId',
a.PaymentStatus,
a.CompletionDate,
a.ContinuationOfId,
pa.StopDate As 'PreviousApprenticeshipStopDate'
FROM @ULNs u
INNER JOIN [dbo].[Apprenticeship] a ON u.ULN = a.ULN
INNER JOIN [dbo].[Commitment] c ON c.Id = a.CommitmentId
INNER JOIN [Providers] p on p.Ukprn = c.ProviderId
INNER JOIN [AccountLegalEntities] ale on ale.Id = c.AccountLegalEntityId
LEFT JOIN [dbo].[Apprenticeship] pa ON pa.Id = a.ContinuationOfId
WHERE
c.Approvals in (3,7)
END