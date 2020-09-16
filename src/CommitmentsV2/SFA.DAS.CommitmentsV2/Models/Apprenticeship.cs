using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using System.Linq;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Apprenticeship : ApprenticeshipBase, ITrackableEntity
    {
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }
        public virtual ICollection<ChangeOfPartyRequest> ChangeOfPartyRequests { get; set; }
        public virtual ApprenticeshipBase Continuation { get; set; }

        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public Originator? PendingUpdateOriginator { get; set; }
        public DateTime? CompletionDate { get; set; }

        public Apprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
            ChangeOfPartyRequests = new List<ChangeOfPartyRequest>();
        }

        public virtual ChangeOfPartyRequest CreateChangeOfPartyRequest(ChangeOfPartyRequestType changeOfPartyType,
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
                throw new DomainException(nameof(ChangeOfPartyRequests),
                    $"Change of Party requires that no Pending or Approved requests exist for Apprenticeship {Id}");
            }
        }

        public virtual void Complete(DateTime completionDate)
        {
            var status = GetApprenticeshipStatus(completionDate);
            if (status != ApprenticeshipStatus.Live && status != ApprenticeshipStatus.Paused && status != ApprenticeshipStatus.Stopped)
            {
                throw new InvalidOperationException("Apprenticeship has to be Live, Paused, or Stopped in order to be completed");
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

        public DraftApprenticeship CreateCopyForChangeOfParty(ChangeOfPartyRequest changeOfPartyRequest, Guid? reservationId)
        {
            var result = new DraftApprenticeship
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                DateOfBirth = this.DateOfBirth,
                Cost = changeOfPartyRequest.Price,
                StartDate = changeOfPartyRequest.StartDate,
                EndDate = changeOfPartyRequest.EndDate ?? EndDate.Value,
                Uln = this.Uln,
                CourseCode = this.CourseCode,
                CourseName = this.CourseName,
                ProgrammeType = this.ProgrammeType,
                EmployerRef = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? string.Empty : this.EmployerRef,
                ProviderRef = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider ? string.Empty : this.ProviderRef,
                ReservationId = reservationId,
                ContinuationOfId = Id,
                OriginalStartDate = OriginalStartDate ?? StartDate
            };

            return result;
        }

        public void EditEndDateOfCompletedRecord(DateTime endDate, ICurrentDateTime currentDate, Party party, UserInfo userInfo)
        {
            if (PaymentStatus != PaymentStatus.Completed)
            {
                throw new DomainException(nameof(EndDate), "Only completed record end date can be changed");
            }

            if (endDate > CompletionDate)
            {
                throw new DomainException(nameof(EndDate), "Planned training end date must be the same as or before the completion payment month");
            }

            if (endDate <= StartDate)
            {
                throw new DomainException(nameof(EndDate), "Planned training end date must be after the planned training start date");
            }

            StartTrackingSession(UserAction.EditEndDateOfCompletedApprentice, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

            ChangeTrackingSession.TrackUpdate(this);

            EndDate = endDate;

            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new ApprenticeshipUpdatedApprovedEvent
            {
                ApprenticeshipId = Id,
                ApprovedOn = currentDate.UtcNow,
                StartDate = StartDate.Value,
                EndDate = EndDate.Value,
                PriceEpisodes = GetPriceEpisodes(),
                TrainingType = ProgrammeType.Value,
                TrainingCode = CourseCode,
                Uln = Uln
            }); 
        }

        private PriceEpisode[] GetPriceEpisodes()
        {
            return PriceHistory
                .Select(x => new PriceEpisode
                {
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    Cost = x.Cost
                }).ToArray();
        }
    }
}