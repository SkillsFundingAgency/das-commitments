using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Apprenticeship : ApprenticeshipBase
    {
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }
        public virtual ICollection<ChangeOfPartyRequest> ChangeOfPartyRequests { get; set; }

        public int? PaymentOrder { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public Originator? PendingUpdateOriginator { get; set; }
        public ApprenticeshipStatus Status
        {
            get
            {
                switch (PaymentStatus)
                {
                    case PaymentStatus.Active:
                        return DateTime.UtcNow < StartDate
                            ? ApprenticeshipStatus.WaitingToStart
                            : ApprenticeshipStatus.Live;
                    case PaymentStatus.Withdrawn:
                        return ApprenticeshipStatus.Stopped;
                    case PaymentStatus.Paused:
                        return ApprenticeshipStatus.Paused;
                    case PaymentStatus.Completed:
                        return ApprenticeshipStatus.Completed;
                    default:
                        return ApprenticeshipStatus.Unknown;
                }
            }
        }

        public Apprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
            ChangeOfPartyRequests = new List<ChangeOfPartyRequest>();
        }

        public ChangeOfPartyRequest CreateChangeOfPartyRequest(ChangeOfPartyRequestType changeOfPartyType,
            Party originatingParty,
            long newPartyId,
            int price,
            DateTime startDate,
            DateTime? endDate,
            UserInfo userInfo,
            DateTime now)
        {
            CheckIsStoppedForChangeOfParty();
            CheckStartDateForChangeOfParty(startDate);
            CheckNoPendingOrApprovedRequestsForChangeOfParty();

            return new ChangeOfPartyRequest(this, changeOfPartyType, originatingParty, newPartyId, price, startDate, endDate, userInfo, now);
        }

        private void CheckIsStoppedForChangeOfParty()
        {
            if (PaymentStatus != PaymentStatus.Withdrawn)
            {
                throw new DomainException(nameof(PaymentStatus), $"Change of Party requires that Apprenticeship {Id} already be stopped but actual status is {PaymentStatus}");
            }
        }

        private void CheckStartDateForChangeOfParty(DateTime startDate)
        {
            if (StopDate > startDate)
            {
                throw new DomainException(nameof(StopDate), $"Change of Party requires that Stop Date of Apprenticeship {Id} ({StopDate}) be before or same as new Start Date of {startDate}");
            }
        }

        private void CheckNoPendingOrApprovedRequestsForChangeOfParty()
        {
            if (ChangeOfPartyRequests.Any(x =>
                x.Status == ChangeOfPartyRequestStatus.Approved || x.Status == ChangeOfPartyRequestStatus.Pending))
            {
                throw new DomainException(nameof(ChangeOfPartyRequests), $"Change of Party requires that no Pending or Approved requests exist for Apprenticeship {Id}");
            }
        }
    }
}