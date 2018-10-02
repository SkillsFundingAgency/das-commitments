CREATE PROCEDURE [dbo].[GetApprovedApprenticeshipsForProvider]
	@id BIGINT
AS
	
	select
	a.Id,
	a.CommitmentId,
	c.EmployerAccountId,
	c.ProviderId,
	c.TransferSenderId,
	c.Reference,
	a.FirstName,
	a.LastName,
	a.DateOfBirth,
	a.NINumber,
	a.ULN,
	a.TrainingType,
	a.TrainingCode,
	a.TrainingName,
	a.Cost, --this is NOT the cost (this is not a valid property of an approved apprenticeship)
	a.StartDate,
	a.EndDate,
	a.PauseDate,
	a.StopDate,
	a.PaymentStatus,
	a.AgreementStatus, -- this should always been bothagreed (this is not a valid property of an approved apprenticeship)
	a.EmployerRef,
	a.ProviderRef,
	a.PendingUpdateOriginator,
	c.ProviderName,
	c.LegalEntityId,
	c.LegalEntityName,
	c.AccountLegalEntityPublicHashedId,
	a.HasHadDataLockSuccess,
	dl.DataLockEventId,
	dl.ErrorCode,
	dl.TriageStatus
	from
	Apprenticeship a
	JOIN Commitment c on c.Id = a.CommitmentId
	LEFT JOIN DataLockStatus dl on dl.ApprenticeshipId = a.Id and dl.[IsResolved] = 0 AND dl.[EventStatus] <> 3 AND dl.[IsExpired] = 0 -- Not expired, resolved, or deleted
	where
	c.ProviderId = @id
    AND NOT a.PaymentStatus IN (0,5); --Not deleted or pre-approved