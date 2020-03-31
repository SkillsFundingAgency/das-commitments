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

        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public Originator? PendingUpdateOriginator { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Apprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
        }

        public virtual void Complete(DateTime completionDate)
        {
            if (this.GetApprenticeshipStatus(completionDate) != ApprenticeshipStatus.Live)
            {
                throw new InvalidOperationException("Apprenticeship has to be live to be completed");
            }

            StartTrackingSession(UserAction.Complete, Party.None, Cohort.EmployerAccountId, Cohort.ProviderId, null);
            ChangeTrackingSession.TrackUpdate(this);
            PaymentStatus = PaymentStatus.Completed;
            CompletionDate = completionDate;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new ApprenticeshipCompletedEvent{ ApprenticeshipId = Id, CompletionDate = completionDate});
        }

        public virtual void UpdateCompletionDate(DateTime completionDate)
        {
            if (PaymentStatus != PaymentStatus.Completed)
            {
                throw new DomainException("CompletionDate", "The completion date can only be updated if Apprenticeship Status is Completed");
            }

            StartTrackingSession(UserAction.UpdateCompletionDate, Party.None, Cohort.EmployerAccountId, Cohort.ProviderId,null);
            ChangeTrackingSession.TrackUpdate(this);
            CompletionDate = completionDate;
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new ApprenticeshipCompletionDateUpdatedEvent { ApprenticeshipId = Id, CompletionDate = completionDate });
        }

        public ApprenticeshipStatus GetApprenticeshipStatus(DateTime? effectiveDate)
        {
            switch (PaymentStatus)
            {
                case PaymentStatus.Active:
                    return (effectiveDate ?? DateTime.UtcNow) < StartDate
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
}