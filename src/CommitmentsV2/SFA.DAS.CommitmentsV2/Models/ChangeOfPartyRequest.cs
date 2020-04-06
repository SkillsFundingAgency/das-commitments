using System;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ChangeOfPartyRequest : Aggregate, ITrackableEntity
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public ChangeOfPartyRequestType ChangeOfPartyType { get; set; }
        public Party OriginatingParty { get; set; }
        public long? AccountLegalEntityId { get; set; }
        public long? ProviderId { get; set; }
        public int Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public ChangeOfPartyRequestStatus Status { get; set; }

        public byte[] RowVersion { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public virtual Apprenticeship Apprenticeship { get; set; }
        //public virtual AccountLegalEntity AccountLegalEntity { get; set; }
        //public virtual Provider Provider { get; set; }

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
            //invariants
            CheckOriginatingParty(originatingParty);
            CheckRequestType(originatingParty, changeOfPartyType);

            //start tracking
            StartTrackingSession(UserAction.CreateChangeOfPartyRequest, originatingParty, apprenticeship.Cohort.AccountLegalEntityId, apprenticeship.Cohort.ProviderId, userInfo);

            //state change
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

            //commit tracking
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();

            //events
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
    }
}
