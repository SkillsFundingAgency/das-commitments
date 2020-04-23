using System;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
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
        public Party OriginatingParty { get; private set; }
        public virtual long? AccountLegalEntityId { get; private set; }
        public virtual long? ProviderId { get; private set; }
        public int Price { get; private set; }
        public DateTime StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public ChangeOfPartyRequestStatus Status { get; private set; }

        public byte[] RowVersion { get; private set; }
        public DateTime LastUpdatedOn { get; private set; }

        public virtual Apprenticeship Apprenticeship { get; private set; }
        public virtual AccountLegalEntity AccountLegalEntity { get; private set; }

        public ChangeOfPartyRequest()
        {
        }

        public ChangeOfPartyRequest(Apprenticeship apprenticeship,
            ChangeOfPartyRequestType changeOfPartyType,
            Party originatingParty,
            long newPartyId,
            int price,
            DateTime startDate,
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

            Publish(() => new ChangeOfPartyRequestCreatedEvent { ChangeOfPartyRequestId = Id });
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

        private void CheckPrice(int price)
        {
            if (price <= 0 || price > Constants.MaximumApprenticeshipCost)
            {
                throw new DomainException(nameof(Price), $"Change of Party for  Apprenticeship {ApprenticeshipId} requires Price between 1 and {Constants.MaximumApprenticeshipCost}; {price} is not valid");
            }
        }

        public virtual Cohort CreateCohort(Apprenticeship apprenticeship, Guid reservationId)
        {
            //determine the providerId (that on the cohort, or in the copr)
            //ditto account and account legal entity

            var accountId = 1;
            var accountLegalEntityId = 1;

            return new Cohort(apprenticeship.Cohort.ProviderId, accountId, accountLegalEntityId, apprenticeship, reservationId, OriginatingParty);
        }
    }
}
