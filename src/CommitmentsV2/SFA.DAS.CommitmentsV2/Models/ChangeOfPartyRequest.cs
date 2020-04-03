using System;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ChangeOfPartyRequest : Aggregate, ITrackableEntity
    {
        public long Id { get; }
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

        //public virtual Apprenticeship Apprenticeship { get; set; }
        //public virtual AccountLegalEntity AccountLegalEntity { get; set; }
        //public virtual Provider Provider { get; set; }

        public ChangeOfPartyRequest(Apprenticeship apprenticeship,
            ChangeOfPartyRequestType changeOfPartyType,
            Party originatingParty,
            long? accountLegalEntityId,
            long? providerId,
            int price,
            DateTime startDate,
            DateTime? endDate)
        {
            
            //todo: invariants

            //start tracking

            //state change
            ApprenticeshipId = apprenticeship.Id;
            ChangeOfPartyType = changeOfPartyType;
            OriginatingParty = originatingParty;
            AccountLegalEntityId = accountLegalEntityId;
            ProviderId = providerId;
            Price = price;
            StartDate = startDate;
            EndDate = endDate;

            Status = ChangeOfPartyRequestStatus.Pending;
            CreatedOn = DateTime.UtcNow;
            LastUpdatedOn = DateTime.UtcNow;

            //commit tracking

            //events

        }
    }
}
