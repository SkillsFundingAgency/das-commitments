using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Apprenticeship : ApprenticeshipBase, ITrackableEntity
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
            if (Status != ApprenticeshipStatus.Live)
            {
                throw new InvalidOperationException("Apprenticeship has to be live to be completed");
            }

            if (completionDate <= StartDate.Value)
            {
                throw new InvalidOperationException("The completion date must be after the apprenticeship start date");
            }

            StartTrackingSession(UserAction.CompletionPayment, Party.None, Cohort.EmployerAccountId, Cohort.ProviderId, null);
            ChangeTrackingSession.TrackUpdate(this);
            PaymentStatus = PaymentStatus.Completed;
            CompletionDate = completionDate;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new ApprenticeshipCompletedEvent{ ApprenticeshipId = Id, CompletionDate = completionDate});
        }

        public virtual void UpdateCompletionDate(DateTime completionDate)
        {
            if (Status != ApprenticeshipStatus.Completed)
            {
                throw new DomainException("CompletionDate", "The completion date can only be updated if Apprenticeship Status is Completed");
            }

            StartTrackingSession(UserAction.CompletionPayment, Party.None, Cohort.EmployerAccountId, Cohort.ProviderId,null);
            ChangeTrackingSession.TrackUpdate(this);
            CompletionDate = completionDate;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new ApprenticeshipCompletionDateUpdatedEvent { ApprenticeshipId = Id, CompletionDate = completionDate });
        }
    }
}