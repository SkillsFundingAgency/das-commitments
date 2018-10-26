CREATE PROCEDURE [dbo].[GetApprovedApprenticeship]
	@id bigint
AS

	select
	a.Id,
	c.Reference as CohortReference,
	c.EmployerAccountId,
	c.ProviderId,
	c.TransferSenderId,
	a.FirstName,
	a.LastName,
	a.DateOfBirth,
	a.ULN,
	a.TrainingType,
	a.TrainingCode,
	a.TrainingName,
	a.StartDate,
	a.EndDate,
	a.PauseDate,
	a.StopDate,
	a.PaymentStatus,
	a.EmployerRef,
	a.ProviderRef,
	a.PendingUpdateOriginator as 'UpdateOriginator',
	c.ProviderName,
	c.LegalEntityId,
	c.LegalEntityName,
	c.AccountLegalEntityPublicHashedId,
	a.HasHadDataLockSuccess,
	--PriceHistory
	ph.Id,
	ph.ApprenticeshipId,
	ph.FromDate,
	ph.ToDate,
	ph.Cost,
	--DataLocks
	dl.Id,
	dl.DataLockEventId,
	dl.DataLockEventDatetime,
	dl.PriceEpisodeIdentifier,
	dl.ApprenticeshipId,
	dl.IlrTrainingCourseCode,
	dl.IlrTrainingType,
	dl.IlrActualStartDate,
	dl.IlrEffectiveFromDate,
	dl.IlrPriceEffectiveToDate,
	dl.IlrTotalCost,
	dl.ErrorCode,
	dl.[Status],
	dl.TriageStatus,
	dl.ApprenticeshipUpdateId,
	dl.IsResolved
from
	Apprenticeship a
	JOIN Commitment c on c.Id = a.CommitmentId
	LEFT JOIN DataLockStatus dl on dl.ApprenticeshipId = a.Id AND dl.[EventStatus] <> 3 AND dl.[IsExpired] = 0 -- Not expired or removed
	LEFT JOIN PriceHistory ph on ph.ApprenticeshipId = a.Id
	where
	a.Id = @id
	AND NOT a.PaymentStatus IN (0,5) --Not deleted or pre-approved
	

