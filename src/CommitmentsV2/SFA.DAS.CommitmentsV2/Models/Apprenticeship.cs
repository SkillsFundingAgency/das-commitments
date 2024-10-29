
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models;

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
    public bool? MadeRedundant { get; set; }

    [NotMapped] public string ApprenticeName => string.Concat(FirstName, " ", LastName);

    public Apprenticeship()
    {
        DataLockStatus = new List<DataLockStatus>();
        PriceHistory = new List<PriceHistory>();
        ChangeOfPartyRequests = new List<ChangeOfPartyRequest>();
    }

    public virtual ChangeOfPartyRequest CreateChangeOfPartyRequest(ChangeOfPartyRequestType changeOfPartyType,
        Party originatingParty,
        long newPartyId,
        int? price,
        DateTime? startDate,
        DateTime? endDate,
        int? employmentPrice,
        DateTime? employmentEndDate,
        DeliveryModel? deliveryModel,
        bool hasOverlappingTrainingDates,
        UserInfo userInfo,
        DateTime now)
    {
        if (!hasOverlappingTrainingDates)
        {
            CheckIsStoppedForChangeOfParty();
            CheckStartDateForChangeOfParty(startDate, changeOfPartyType, originatingParty);
        }

        CheckNoPendingOrApprovedRequestsForChangeOfParty();

        return new ChangeOfPartyRequest(this, changeOfPartyType, originatingParty, newPartyId, price, startDate, endDate, employmentPrice, employmentEndDate, deliveryModel, hasOverlappingTrainingDates, userInfo, now);
    }

    internal void ResolveTrainingDateRequest(long draftApprenticeshipId, OverlappingTrainingDateRequestResolutionType resolutionType)
    {
        var oltdRequest = OverlappingTrainingDateRequests
            .FirstOrDefault(x => x.DraftApprenticeshipId == draftApprenticeshipId
                                 && x.Status == OverlappingTrainingDateRequestStatus.Pending);

        if (oltdRequest == null)
        {
            return;
        }
            
        oltdRequest.ResolutionType = resolutionType;
        oltdRequest.Status = resolutionType == OverlappingTrainingDateRequestResolutionType.ApprenticeshipIsStillActive ||
                             resolutionType == OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopDateIsCorrect ||
                             resolutionType == OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateIsCorrect
            ? OverlappingTrainingDateRequestStatus.Rejected
            : OverlappingTrainingDateRequestStatus.Resolved;

        oltdRequest.ActionedOn = DateTime.UtcNow;

        if (oltdRequest.Status == OverlappingTrainingDateRequestStatus.Rejected)
        {
            Publish(() => new OverlappingTrainingDateRequestRejectedEvent
            {
                OverlappingTrainingDateRequestId = oltdRequest.Id
            });
        }
        else if (resolutionType != OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipUpdated &&
                 resolutionType != OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipDeleted)
        {
            Publish(() =>
                new OverlappingTrainingDateResolvedEvent(draftApprenticeshipId,
                    oltdRequest.DraftApprenticeship.CommitmentId));
        }
    }

    private void CheckIsStoppedForChangeOfParty()
    {
        if (PaymentStatus != PaymentStatus.Withdrawn)
        {
            throw new DomainException(nameof(PaymentStatus), $"Change of Party requires that Apprenticeship {Id} already be stopped but actual status is {PaymentStatus}");
        }
    }

    private void CheckStartDateForChangeOfParty(DateTime? startDate, ChangeOfPartyRequestType changeOfPartyType, Party originatingParty)
    {
        if (changeOfPartyType == ChangeOfPartyRequestType.ChangeProvider && originatingParty == Party.Employer) return;
        if (startDate == null || StopDate > startDate)
        {
            throw new DomainException(nameof(StopDate), $"Change of Party requires that Stop Date of Apprenticeship {Id} ({StopDate}) be before or same as new Start Date of {startDate}");
        }
    }

    public void ApplyApprenticeshipUpdate(Party party, UserInfo userInfo, ICurrentDateTime currentDateTime)
    {
        StartTrackingSession(UserAction.Updated, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

        var update = ApprenticeshipUpdate.First(x => x.Status == ApprenticeshipUpdateStatus.Pending);
        ChangeTrackingSession.TrackUpdate(update);
        ChangeTrackingSession.TrackUpdate(this);

        ApplyApprenticeshipUpdatesToApprenticeship(update, currentDateTime.UtcNow);
        PendingUpdateOriginator = null;
        update.Status = ApprenticeshipUpdateStatus.Approved;

        update.ResolveDataLocks();

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() =>
            new ApprenticeshipUpdatedApprovedEvent
            {
                ApprenticeshipId = Id,
                ApprovedOn = currentDateTime.UtcNow,
                StartDate = StartDate.Value,
                EndDate = EndDate.Value,
                PriceEpisodes = PriceHistory.Select(x => new PriceEpisode
                {
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    Cost = x.Cost
                }).ToArray(),
                TrainingType = (ProgrammeType)ProgrammeType,
                TrainingCode = CourseCode,
                StandardUId = StandardUId,
                TrainingCourseVersion = TrainingCourseVersion,
                TrainingCourseOption = TrainingCourseOption,
                Uln = Uln,
                DeliveryModel = DeliveryModel ?? Types.DeliveryModel.Regular,
                EmploymentEndDate = FlexibleEmployment?.EmploymentEndDate,
                EmploymentPrice = FlexibleEmployment?.EmploymentPrice
            });
    }

    public void AcceptDataLocks(Party party, DateTime acceptedOn, List<long> dataLockEventIds, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.AcceptDataLockChange, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
        foreach (var dataLockEventId in dataLockEventIds)
        {
            var dataLockStatus = DataLockStatus.SingleOrDefault(p => p.DataLockEventId == dataLockEventId);
            if (dataLockStatus == null)
            {
                continue;
            }
                
            ChangeTrackingSession.TrackUpdate(dataLockStatus);
            dataLockStatus.IsResolved = true;
        }

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new
            DataLockTriageApprovedEvent
            {
                ApprenticeshipId = Id,
                ApprovedOn = acceptedOn,
                PriceEpisodes = PriceHistory.Select(x => new PriceEpisode
                {
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    Cost = x.Cost
                }).ToArray(),
                TrainingCode = CourseCode,
                TrainingType = ProgrammeType.Value
            });
    }

    public void RejectDataLocks(Party party, List<long> dataLockEventIds, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.RejectDataLockChange, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
        foreach (var dataLockEventId in dataLockEventIds)
        {
            var dataLockStatus = DataLockStatus.SingleOrDefault(p => p.DataLockEventId == dataLockEventId);
            if (dataLockStatus == null)
            {
                continue;
            }
                
            ChangeTrackingSession.TrackUpdate(dataLockStatus);
            dataLockStatus.TriageStatus = TriageStatus.Unknown;
        }

        ChangeTrackingSession.CompleteTrackingSession();
    }

    public void TriageDataLocks(Party party, List<long> dataLockEventIds, TriageStatus triageStatus, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.TriageDataLocks, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
        foreach (var dataLockEventId in dataLockEventIds)
        {
            var dataLockStatus = DataLockStatus.SingleOrDefault(p => p.DataLockEventId == dataLockEventId);
            if (dataLockStatus == null)
            {
                continue;
            }
                
            ChangeTrackingSession.TrackUpdate(dataLockStatus);
            dataLockStatus.TriageStatus = triageStatus;
        }

        ChangeTrackingSession.CompleteTrackingSession();
    }

    public void ReplacePriceHistory(Party party, List<PriceHistory> currentPriceHistory, List<PriceHistory> updatedPriceHistory, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.TriageDataLocks, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo, Id);
        
        foreach (var priceHistory in currentPriceHistory.Where(priceHistory => updatedPriceHistory.TrueForAll(x => x.Cost != priceHistory.Cost)))
        {
            ChangeTrackingSession.TrackDelete(priceHistory);
        }

        ChangeTrackingSession.CompleteTrackingSession();

        StartTrackingSession(UserAction.TriageDataLocks, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo, Id);
        foreach (var priceHistory in updatedPriceHistory)
        {
            var changedPriceHistory = currentPriceHistory.Find(x => x.Cost == priceHistory.Cost && x.FromDate == priceHistory.FromDate);

            if (changedPriceHistory != null)
            {
                ChangeTrackingSession.TrackUpdate(changedPriceHistory);

                changedPriceHistory.FromDate = priceHistory.FromDate;
                changedPriceHistory.ToDate = priceHistory.ToDate;
                changedPriceHistory.Cost = priceHistory.Cost;
            }
            else
            {
                ChangeTrackingSession.TrackInsert(priceHistory);
            }
        }

        ChangeTrackingSession.CompleteTrackingSession();

        PriceHistory = updatedPriceHistory;
    }

        public void UpdateCourse(Party party, string courseCode, string courseName, ProgrammeType programmeType, UserInfo userInfo, string standardUId, string version, DateTime approvedOn)
        {
            StartTrackingSession(UserAction.UpdateCourse, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            CourseCode = courseCode;
            CourseName = courseName;
            ProgrammeType = programmeType;
            StandardUId = standardUId;
            TrainingCourseVersion = version;
            TrainingCourseVersionConfirmed = version != null;
            TrainingCourseOption = null;

        Publish(() =>
            new ApprenticeshipUpdatedApprovedEvent
            {
                ApprenticeshipId = Id,
                StandardUId = standardUId,
                StartDate = StartDate.Value,
                EndDate = EndDate.Value,
                PriceEpisodes = GetPriceEpisodes(),
                TrainingType = ProgrammeType.Value,
                TrainingCode = CourseCode,
                ApprovedOn = approvedOn,
                TrainingCourseVersion = TrainingCourseVersion,
                TrainingCourseOption = TrainingCourseOption,
                Uln = Uln
            });

        ChangeTrackingSession.CompleteTrackingSession();
    }

    public void RejectApprenticeshipUpdate(Party party, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.Updated, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

        var update = ApprenticeshipUpdate.First(x => x.Status == ApprenticeshipUpdateStatus.Pending);
        ChangeTrackingSession.TrackUpdate(update);
        ChangeTrackingSession.TrackUpdate(this);

        PendingUpdateOriginator = null;
        update.Status = ApprenticeshipUpdateStatus.Rejected;

        update.ResetDataLocks();

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() =>
            new ApprenticeshipUpdateRejectedEvent
            {
                ApprenticeshipId = Id,
                AccountId = Cohort.EmployerAccountId,
                ProviderId = Cohort.ProviderId
            });
    }

    public void UndoApprenticeshipUpdate(Party party, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.Updated, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

        var update = ApprenticeshipUpdate.First(x => x.Status == ApprenticeshipUpdateStatus.Pending);
        ChangeTrackingSession.TrackUpdate(update);
        ChangeTrackingSession.TrackUpdate(this);

        PendingUpdateOriginator = null;
        update.Status = ApprenticeshipUpdateStatus.Deleted;

        update.ResetDataLocks();

        ChangeTrackingSession.CompleteTrackingSession();
    }

    public List<PriceHistory> CreatePriceHistory(
        IEnumerable<DataLockStatus> dataLocksToBeUpdated,
        IEnumerable<DataLockStatus> dataLockPasses)
    {
        var newPriceHistory =
            dataLocksToBeUpdated.Concat(dataLockPasses)
                .Select(
                    dataLockStatus =>
                        new PriceHistory
                        {
                            ApprenticeshipId = Id,
                            Cost = (decimal)dataLockStatus.IlrTotalCost,
                            FromDate = (DateTime)dataLockStatus.IlrEffectiveFromDate,
                            ToDate = null
                        })
                .DistinctBy(x => new { x.Cost, x.FromDate })
                .OrderBy(x => x.FromDate)
                .ToArray();

        for (var index = 0; index < newPriceHistory.Length - 1; index++)
        {
            newPriceHistory[index].ToDate = newPriceHistory[index + 1].FromDate.AddDays(-1);
        }

        return newPriceHistory.ToList();
    }

    private void ApplyApprenticeshipUpdatesToApprenticeship(ApprenticeshipUpdate update, DateTime approvedOn)
    {
        if (!string.IsNullOrEmpty(update.FirstName))
        {
            FirstName = update.FirstName;
        }

        if (!string.IsNullOrEmpty(update.LastName))
        {
            LastName = update.LastName;
        }

        if (!string.IsNullOrEmpty(update.Email))
        {
            Email = update.Email;

            Publish(() =>
                new ApprenticeshipUpdatedEmailAddressEvent
                {
                    ApprenticeshipId = Id,
                    ApprovedOn = approvedOn,
                });
        }

        if (update.DeliveryModel.HasValue)
        {
            DeliveryModel = update.DeliveryModel;
            FlexibleEmployment ??= new FlexibleEmployment();
            if (DeliveryModel == Types.DeliveryModel.Regular)
            {
                FlexibleEmployment.EmploymentEndDate = null;
                FlexibleEmployment.EmploymentPrice = null;
            }
        }

        if (update.EmploymentEndDate.HasValue)
        {
            FlexibleEmployment.EmploymentEndDate = update.EmploymentEndDate;
        }

        if (update.EmploymentPrice.HasValue)
        {
            FlexibleEmployment.EmploymentPrice = update.EmploymentPrice;
        }

        if (update.TrainingType.HasValue)
        {
            ProgrammeType = update.TrainingType;

            if (update.TrainingType.Value == Types.ProgrammeType.Framework)
            {
                TrainingCourseVersion = null;
                TrainingCourseVersionConfirmed = false;
                TrainingCourseOption = null;
                StandardUId = null;
            }
        }

        if (!string.IsNullOrEmpty(update.TrainingCode))
        {
            CourseCode = update.TrainingCode;
        }

        if (!string.IsNullOrEmpty(update.TrainingName))
        {
            CourseName = update.TrainingName;
        }

        if (!string.IsNullOrEmpty(update.TrainingCourseVersion))
        {
            TrainingCourseVersion = update.TrainingCourseVersion;
            TrainingCourseVersionConfirmed = true;
        }

        // ApprenticeshipUpdate.TrainingCourseOption can be null when
        // a - the option hasn't changed
        // b - the new course doesn't have any options
        // If the training course or version has changed then the option can be set to the chosen option, string.Empty (Choose later) or null
        // If the training course and version has not changed then the option can only be updated to the chosen option or string.Empty
        // Otherwise the course has not changed and the option is null then the option should not be changed
        var shouldUpdateOption = !string.IsNullOrEmpty(update.TrainingCode) || !string.IsNullOrEmpty(update.TrainingCourseVersion) || update.TrainingCourseOption != null;

        if (shouldUpdateOption)
        {
            TrainingCourseOption = update.TrainingCourseOption;
        }

        if (!string.IsNullOrEmpty(update.StandardUId))
        {
            StandardUId = update.StandardUId;
        }

        if (update.DateOfBirth.HasValue)
        {
            DateOfBirth = update.DateOfBirth;
        }

        if (update.StartDate.HasValue)
        {
            StartDate = update.StartDate;
        }

            if (update.ActualStartDate.HasValue)
            {
	            ActualStartDate = update.ActualStartDate;
            }

        if (update.EndDate.HasValue)
        {
            EndDate = update.EndDate;
        }

        UpdatePrice(update);
    }

    private void UpdatePrice(ApprenticeshipUpdate update)
    {
        if (update.Cost.HasValue)
        {
            if (PriceHistory.Count != 1)
                throw new InvalidOperationException("Multiple Prices History Items not expected.");

            Cost = update.Cost.Value;
            PriceHistory.First().Cost = update.Cost.Value;
        }

        if (!update.StartDate.HasValue)
        {
            return;
        }
            
        var pH = PriceHistory.First();
        if (PriceHistory.Count != 1)
            throw new InvalidOperationException("Multiple Prices History Items not expected.");

        pH.FromDate = update.ActualStartDate ?? update.StartDate.Value;
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

        Publish(() => new ApprenticeshipCompletedEvent { ApprenticeshipId = Id, CompletionDate = completionDate });
    }

    public virtual void UpdateCompletionDate(DateTime completionDate)
    {
        if (PaymentStatus != PaymentStatus.Completed)
        {
            throw new DomainException(nameof(CompletionDate), "The completion date can only be updated if Apprenticeship Status is Completed");
        }

        StartTrackingSession(UserAction.UpdateCompletionDate, Party.None, Cohort.EmployerAccountId, Cohort.ProviderId, null);
        ChangeTrackingSession.TrackUpdate(this);
        CompletionDate = completionDate;
        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new ApprenticeshipCompletionDateUpdatedEvent { ApprenticeshipId = Id, CompletionDate = completionDate });
    }

    public ApprenticeshipStatus GetApprenticeshipStatus(DateTime? effectiveDate)
    {
        switch (PaymentStatus)
        {
            //case PaymentStatus.PendingApproval: //TODO : Need to Check -- REMOVE LATER
            //    return ApprenticeshipStatus.WaitingToStart;
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
            Email = this.Email,
            EmailAddressConfirmed = this.EmailAddressConfirmed,
            DateOfBirth = this.DateOfBirth,
            Cost = changeOfPartyRequest.Price,
            StartDate = changeOfPartyRequest.StartDate,
            EndDate = changeOfPartyRequest.EndDate,
            Uln = this.Uln,
            DeliveryModel = changeOfPartyRequest.DeliveryModel ?? DeliveryModel,
            CourseCode = this.CourseCode,
            CourseName = this.CourseName,
            ProgrammeType = this.ProgrammeType,
            EmployerRef = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer ? string.Empty : this.EmployerRef,
            ProviderRef = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider ? string.Empty : this.ProviderRef,
            ReservationId = reservationId,
            ContinuationOfId = Id,
            OriginalStartDate = OriginalStartDate ?? StartDate,
            StandardUId = this.StandardUId,
            TrainingCourseVersion = this.TrainingCourseVersion,
            TrainingCourseVersionConfirmed = this.TrainingCourseVersionConfirmed,
            TrainingCourseOption = this.TrainingCourseOption,
            FlexibleEmployment = CreateFlexibleEmploymentForChangeOfParty(changeOfPartyRequest),
            ApprenticeshipConfirmationStatus = ApprenticeshipConfirmationStatus?.Copy(),
            IsOnFlexiPaymentPilot = this.IsOnFlexiPaymentPilot,
            EmployerHasEditedCost = this.EmployerHasEditedCost,
            RecognisePriorLearning = this.RecognisePriorLearning,
            TrainingTotalHours = this.TrainingTotalHours,
        };

        if (result.RecognisePriorLearning == true)
        {
            result.PriorLearning = new ApprenticeshipPriorLearning
            {
                DurationReducedByHours = this.PriorLearning.DurationReducedByHours,
                IsDurationReducedByRpl = this.PriorLearning.IsDurationReducedByRpl,
                DurationReducedBy = this.PriorLearning.DurationReducedBy,
                PriceReducedBy = this.PriorLearning.PriceReducedBy,
            };
        }

        return result;
    }

    private FlexibleEmployment CreateFlexibleEmploymentForChangeOfParty(ChangeOfPartyRequest changeOfPartyRequest)
    {
        if (DeliveryModel != Types.DeliveryModel.PortableFlexiJob) return null;

        // TODO Should this be limited to CoE
        return new FlexibleEmployment
        {
            EmploymentPrice = changeOfPartyRequest.EmploymentPrice.Value,
            EmploymentEndDate = changeOfPartyRequest.EmploymentEndDate.Value
        };
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
            StandardUId = StandardUId,
            TrainingCourseVersion = TrainingCourseVersion,
            TrainingCourseOption = TrainingCourseOption,
            Uln = Uln,
            DeliveryModel = DeliveryModel ?? Types.DeliveryModel.Regular,
        });
    }

    public void PauseApprenticeship(ICurrentDateTime currentDateTime, Party party, UserInfo userInfo)
    {
        var pausedDate = currentDateTime.UtcNow;
        if (PaymentStatus != PaymentStatus.Active)
        {
            throw new DomainException(nameof(EndDate), "Only Active record can be paused");
        }

        StartTrackingSession(UserAction.PauseApprenticeship, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

        ChangeTrackingSession.TrackUpdate(this);

        PaymentStatus = PaymentStatus.Paused;
        PauseDate = pausedDate;

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new ApprenticeshipPausedEvent
        {
            ApprenticeshipId = Id,
            PausedOn = pausedDate
        });
    }

    public void ResumeApprenticeship(ICurrentDateTime currentDateTime, Party party, UserInfo userInfo)
    {
        var resumedDate = currentDateTime.UtcNow;
        if (PaymentStatus != PaymentStatus.Paused)
        {
            throw new DomainException(nameof(PaymentStatus), "Only paused record can be activated");
        }

        StartTrackingSession(UserAction.ResumeApprenticeship, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

        ChangeTrackingSession.TrackUpdate(this);
        PaymentStatus = PaymentStatus.Active;
        PauseDate = null;

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new ApprenticeshipResumedEvent
        {
            ApprenticeshipId = Id,
            ResumedOn = resumedDate
        });
    }

    public void ConfirmEmailAddress(string email)
    {
        if (EmailAddressConfirmed == true)
            return;

        ChangeEmailAddress(email);

        EmailAddressConfirmed = true;
    }

    public void ChangeEmailAddress(string email)
    {
        if (!Email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
            Email = email;
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

    private void ValidateApprenticeshipForStop(DateTime stopDate, long accountId, ICurrentDateTime currentDate)
    {
        if (PaymentStatus == PaymentStatus.Completed || PaymentStatus == PaymentStatus.Withdrawn)
        {
            throw new DomainException(nameof(PaymentStatus), "Apprenticeship must be Active or Paused. Unable to stop apprenticeship");
        }

        if (Cohort.EmployerAccountId != accountId)
        {
            throw new DomainException(nameof(accountId), $"Employer {accountId} not authorised to access commitment {Cohort.Id}, expected employer {Cohort.EmployerAccountId}");
        }

        if (this.IsWaitingToStart(currentDate))
        {
            if (stopDate.Date != StartDate.Value.Date)
                throw new DomainException(nameof(stopDate), "Invalid stop date. Date should be value of start date if training has not started.");
        }
        else
        {
            // When asking for a stop date, only a month and year are provded by the UI, The day is not supplied.
            // As a result, when constructing comparisons, it is clear the dates must also be of the same format.
            if (stopDate.Date > new DateTime(currentDate.UtcNow.Year, currentDate.UtcNow.Month, 1))
            {
                throw new DomainException(nameof(stopDate), "Invalid Stop Date. Stop date cannot be in the future and must be the 1st of the month.");
            }

            if (stopDate.Date < new DateTime(StartDate.Value.Year, StartDate.Value.Month, 1))
            {
                throw new DomainException(nameof(stopDate), "Invalid Stop Date. Stop date cannot be before the apprenticeship has started.");
            }
        }
    }

    public void StopApprenticeship(DateTime stopDate, long accountId, bool madeRedundant, UserInfo userInfo, ICurrentDateTime currentDate, Party party)
    {
        ValidateApprenticeshipForStop(stopDate, accountId, currentDate);

        StartTrackingSession(UserAction.StopApprenticeship, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);

        ChangeTrackingSession.TrackUpdate(this);

        PaymentStatus = PaymentStatus.Withdrawn;
        StopDate = stopDate;
        MadeRedundant = madeRedundant;

        ResolveDatalocks(stopDate);

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new ApprenticeshipStoppedEvent
        {
            AppliedOn = currentDate.UtcNow,
            ApprenticeshipId = Id,
            StopDate = stopDate
        });
    }

    public void UpdateEmployerReference(string employerReference, Party party, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.EditedApprenticeship, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);

        ValidateForEmployerReference(party);

        EmployerRef = employerReference;

        ChangeTrackingSession.CompleteTrackingSession();
    }

    private static void ValidateForEmployerReference(Party party)
    {
        if (party != Party.Employer)
        {
            throw new InvalidOperationException("Employer reference can only be changed by employer ");
        }
    }

    private static void ValidateForProvider(Party party)
    {
        if (party != Party.Provider)
        {
            throw new InvalidOperationException("Can only be changed by provider ");
        }
    }

    public void CreateApprenticeshipUpdate(ApprenticeshipUpdate apprenitceshipUpdate, Party party)
    {
        PendingUpdateOriginator = party == Party.Employer ? Originator.Employer : Originator.Provider;
        ApprenticeshipUpdate.Add(apprenitceshipUpdate);
    }

    public void UpdateProviderReference(string providerReference, Party party, UserInfo userInfo)
    {
        StartTrackingSession(UserAction.EditedApprenticeship, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);

        ValidateForProvider(party);

        ProviderRef = providerReference;

        ChangeTrackingSession.CompleteTrackingSession();
    }

    public void UpdateULN(string uln, Party party, DateTime currentDateTime, UserInfo userInfo)
    {
        ValidateForProvider(party);
        StartTrackingSession(UserAction.EditedApprenticeship, party, Cohort.EmployerAccountId, Cohort.ProviderId, userInfo);
        ChangeTrackingSession.TrackUpdate(this);
        Uln = uln;
        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new ApprenticeshipUlnUpdatedEvent(Id, uln, currentDateTime));
    }

    public void ApprenticeshipStopDate(UpdateApprenticeshipStopDateCommand command, ICurrentDateTime currentDate, Party party)
    {
        StartTrackingSession(UserAction.UpdateApprenticeshipStopDate, party, Cohort.EmployerAccountId, Cohort.ProviderId, command.UserInfo);

        ChangeTrackingSession.TrackUpdate(this);
        if (PaymentStatus != PaymentStatus.Completed)
        {
            StopDate = command.StopDate;
        }

        ResolveDatalocks(command.StopDate);

        ChangeTrackingSession.CompleteTrackingSession();

        Publish(() => new ApprenticeshipStopDateChangedEvent
        {
            StopDate = command.StopDate,
            ApprenticeshipId = command.ApprenticeshipId,
            ChangedOn = currentDate.UtcNow
        });
    }

    private void ResolveDatalocks(DateTime stopDate)
    {
        IEnumerable<DataLockStatus> dataLocks;
        if (stopDate == StartDate)
        {
            dataLocks = DataLockStatus.Where(x => x.EventStatus != EventStatus.Removed && !x.IsExpired);
        }
        else
        {
            dataLocks = DataLockStatus.Where(x => x.EventStatus != EventStatus.Removed &&
                                                  !x.IsExpired && !x.IsResolved &&
                                                  x.TriageStatus == TriageStatus.Restart
                                                  && x.WithCourseError()).ToList();
        }

        foreach (var dataLock in dataLocks)
        {
            ChangeTrackingSession.TrackUpdate(dataLock);
            dataLock.Resolve();
        }
    }

    public static ConfirmationStatus? DisplayConfirmationStatus(string email, DateTime? confirmedOn, DateTime? overdueOn)
    {
        if (email == null)
        {
            return null;
        }

        if (confirmedOn.HasValue)
        {
            return ConfirmationStatus.Confirmed;
        }

        if (overdueOn.HasValue && DateTime.UtcNow > overdueOn)
        {
            return ConfirmationStatus.Overdue;
        }

        return ConfirmationStatus.Unconfirmed;
    }
}