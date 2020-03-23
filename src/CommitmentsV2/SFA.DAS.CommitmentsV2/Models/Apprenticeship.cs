using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Apprenticeship : ApprenticeshipBase
    {
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }

        public int? PaymentOrder { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public Originator? PendingUpdateOriginator { get; set; }
        public virtual ApprenticeshipStatus Status
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

        public DateTime? CompletionDate { get; set; }

        public Apprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
        }

        public virtual void Complete(DateTime completionDate)
        {
            throw new NotImplementedException();
        }

        public virtual void UpdateCompletionDate(DateTime completionDate)
        {
            throw new NotImplementedException();
        }
    }
}