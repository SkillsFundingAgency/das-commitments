﻿CREATE PROCEDURE [dbo].[GetApprovedApprenticeshipsForProvider]
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
	0 as AgreementStatus, -- this should always been bothagreed (this is not a valid property of an approved apprenticeship)
	a.EmployerRef,
	a.ProviderRef,
	a.PendingUpdateOriginator as 'UpdateOriginator',
	a.ReservationId,
	p.[Name] as 'ProviderName',
	ale.LegalEntityId,
	ale.[Name] as 'LegalEntityName',
	ale.PublicHashedId as 'AccountLegalEntityPublicHashedId',
	a.HasHadDataLockSuccess,
	dl.DataLockEventId,
	dl.ErrorCode,
	dl.TriageStatus
	from
	Apprenticeship a
	JOIN Commitment c on c.Id = a.CommitmentId
	INNER JOIN [Providers] p on p.Ukprn = c.ProviderId
	INNER JOIN [AccountLegalEntities] ale on ale.Id = c.AccountLegalEntityId
	LEFT JOIN DataLockStatus dl on dl.ApprenticeshipId = a.Id and dl.[IsResolved] = 0 AND dl.[EventStatus] <> 3 AND dl.[IsExpired] = 0 -- Not expired, resolved, or deleted
	where
	c.ProviderId = @id
    AND NOT a.PaymentStatus IN (0,5); --Not deleted or pre-approved