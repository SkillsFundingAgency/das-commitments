using System;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ChangeOfPartyRequest : Aggregate, ITrackableEntity
    {
        public virtual long Id { get; private set; }
        public virtual long ApprenticeshipId { get; private set; }
        public virtual ChangeOfPartyRequestType ChangeOfPartyType { get; private set; }
        public virtual Party OriginatingParty { get; private set; }
        public virtual long? AccountLegalEntityId { get; private set; }
        public virtual long? ProviderId { get; private set; }
        public int? Price { get; private set; }
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public virtual ChangeOfPartyRequestStatus Status { get; private set; }
        public virtual long? CohortId { get; private set; }
        public DateTime? ActionedOn { get; private set; }

        public byte[] RowVersion { get; private set; }
        public DateTime LastUpdatedOn { get; private set; }
        public virtual long? NewApprenticeshipId { get; private set; }

        public virtual Apprenticeship Apprenticeship { get; private set; }
        public virtual AccountLegalEntity AccountLegalEntity { get; private set; }
        public virtual Cohort Cohort { get; private set; }

        public ChangeOfPartyRequest()
        {
        }

        public ChangeOfPartyRequest(Apprenticeship apprenticeship,
            ChangeOfPartyRequestType changeOfPartyType,
            Party originatingParty,
            long newPartyId,
            int? price,
            DateTime? startDate,
            DateTime? endDate,
            UserInfo userInfo,
            DateTime now)
        {
            CheckOriginatingParty(originatingParty);
            CheckRequestType(originatingParty, changeOfPartyType);
            CheckPrice(price);

            StartTrackingSession(UserAction.CreateChangeOfPartyRequest, originatingParty, apprenticeship.Cohort.AccountLegalEntityId, apprenticeship.Cohort.ProviderId, userInfo);

            ApprenticeshipId = apprenticeship.Id;
            ChangeOfPartyType = changeOfPartyType;
            OriginatingParty = originatingParty;
            AccountLegalEntityId = changeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? newPartyId : default(long?);
            ProviderId = changeOfPartyType == ChangeOfPartyRequestType.ChangeProvider ? newPartyId : default(long?);
            Price = price;
            StartDate = startDate;
            EndDate = endDate;
            Status = ChangeOfPartyRequestStatus.Pending;
            CreatedOn = now;
            LastUpdatedOn = now;

            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new ChangeOfPartyRequestCreatedEvent (Id, userInfo));
        }

        private void CheckOriginatingParty(Party originatingParty)
        {
            if (originatingParty != Party.Provider && originatingParty != Party.Employer)
            {
                throw new DomainException(nameof(OriginatingParty), "Only Provider or Employer can create a ChangeOfPartyRequest");
            }
        }

        private void CheckRequestType(Party originatingParty, ChangeOfPartyRequestType requestType)
        {
            var validRequestType = GetValidRequestTypeForOriginator(originatingParty);

            if (requestType != validRequestType)
            {
                throw new DomainException(nameof(ChangeOfPartyRequestType), $"{originatingParty} can only create requests of type {validRequestType}");
            }
        }

        private ChangeOfPartyRequestType GetValidRequestTypeForOriginator(Party originatingParty)
        {
            switch (originatingParty)
            {
                case Party.Provider:
                    return ChangeOfPartyRequestType.ChangeEmployer;
                case Party.Employer:
                    return ChangeOfPartyRequestType.ChangeProvider;
                default:
                    throw new ArgumentException($"Invalid ChangeOfParty originator: {originatingParty}",nameof(originatingParty));
            }
        }

        private void CheckPrice(int? price)
        {
            if (price != null && (price <= 0 || price > Constants.MaximumApprenticeshipCost))
            {
                throw new DomainException(nameof(Price), $"Change of Party for  Apprenticeship {ApprenticeshipId} requires Price between 1 and {Constants.MaximumApprenticeshipCost}; {price} is not valid");
            }
        }

        public virtual Cohort CreateCohort(Apprenticeship apprenticeship, Guid? reservationId, UserInfo userInfo)
        {
            long providerId;
            long accountId;
            long accountLegalEntityId;

            switch (ChangeOfPartyType)
            {
                case ChangeOfPartyRequestType.ChangeEmployer:
                    providerId = apprenticeship.Cohort.ProviderId;
                    accountId = AccountLegalEntity.AccountId;
                    accountLegalEntityId = AccountLegalEntityId.Value;
                    break;
                case ChangeOfPartyRequestType.ChangeProvider:
                    providerId = ProviderId.Value;
                    accountId = apprenticeship.Cohort.EmployerAccountId;
                    accountLegalEntityId = apprenticeship.Cohort.AccountLegalEntityId;
                    break;
                default:
                    throw new Exception("Invalid ChangeOfPartyType");
            }

            return new Cohort(providerId, accountId, accountLegalEntityId, apprenticeship, reservationId, this, userInfo);
        }

        public virtual void SetCohort(Cohort cohort, UserInfo userInfo)
        {
            CheckCohortIdNotSet(cohort.Id);

            StartTrackingSession(UserAction.SetCohortId, OriginatingParty, cohort.EmployerAccountId, cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            CohortId = cohort.Id;

            ChangeTrackingSession.CompleteTrackingSession();
        }

        public virtual void SetNewApprenticeship(Apprenticeship apprenticeship, UserInfo userInfo, Party modifyingParty)
        {
            CheckNewApprenticeshipIdNotSet(apprenticeship.Id);

            StartTrackingSession(UserAction.SetNewApprenticeshipId, modifyingParty, apprenticeship.Cohort.EmployerAccountId, apprenticeship.Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            NewApprenticeshipId = apprenticeship.Id;

            ChangeTrackingSession.CompleteTrackingSession();
        }

        private void CheckCohortIdNotSet(long newValue)
        {
            if (CohortId.HasValue)
            {
                throw new InvalidOperationException($"ChangeOfPartyRequest already has CohortId value of {CohortId.Value} set; cannot set to {newValue}");
            }
        }

        private void CheckNewApprenticeshipIdNotSet(long newValue)
        {
            if (NewApprenticeshipId.HasValue)
            {
                throw new InvalidOperationException($"ChangeOfPartyRequest already has NewApprenticeshipId value of {NewApprenticeshipId.Value} set; cannot set to {newValue}");
            }
        }

        public virtual void Approve(Party modifyingParty, UserInfo userInfo)
        {
            CheckStatusIsPending();

            StartTrackingSession(UserAction.ApproveChangeOfPartyRequest, modifyingParty, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            Status = ChangeOfPartyRequestStatus.Approved;
            ActionedOn = DateTime.UtcNow;

            ChangeTrackingSession.CompleteTrackingSession();
        }

        public virtual void Withdraw(Party modifyingParty, UserInfo userInfo)
        {
            CheckStatusIsPending();

            StartTrackingSession(UserAction.WithdrawChangeOfPartyRequest, modifyingParty, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            Status = ChangeOfPartyRequestStatus.Withdrawn;
            ActionedOn = DateTime.UtcNow;

            ChangeTrackingSession.CompleteTrackingSession();
        }

        public virtual void Reject(Party modifyingParty, UserInfo userInfo)
        {
            CheckStatusIsPending();

            StartTrackingSession(UserAction.RejectChangeOfPartyRequest, modifyingParty, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            Status = ChangeOfPartyRequestStatus.Rejected;
            ActionedOn = DateTime.UtcNow;

            ChangeTrackingSession.CompleteTrackingSession();
        }

        private void CheckStatusIsPending()
        {
            if (Status != ChangeOfPartyRequestStatus.Pending)
            {
                throw new InvalidOperationException($"ChangeOfPartyRequest has status of {Status} and cannot be modified");
            }
        }
    }
}
