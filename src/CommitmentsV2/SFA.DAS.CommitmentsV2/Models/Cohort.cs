using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Services;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Cohort : Aggregate, ITrackableEntity
    {
        public Cohort()
        {
            Apprenticeships = new HashSet<ApprenticeshipBase>();
            Messages = new HashSet<Message>();
            TransferRequests = new HashSet<TransferRequest>();
            LastUpdatedOn = DateTime.UtcNow;
        }

        private Cohort(long providerId,
            long accountId,
            long accountLegalEntityId,
            long? transferSenderId,
            Party originatingParty, UserInfo userInfo) : this()
        {
            CheckIsEmployerOrProvider(originatingParty);

            EmployerAccountId = accountId;
            AccountLegalEntityId = accountLegalEntityId;
            ProviderId = providerId;
            TransferSenderId = transferSenderId;
            IsDraft = true;

            // Reference cannot be set until we've saved the commitment (as we need the Id) but it's non-nullable so we'll use a temp value
            Reference = "";
            Originator = originatingParty.ToOriginator();
            UpdatedBy(originatingParty, userInfo);
            LastUpdatedOn = DateTime.UtcNow;
            CommitmentStatus = CommitmentStatus.New;
            CreatedOn = DateTime.UtcNow;
            LastAction = LastAction.None;
        }

        /// <summary>
        /// Creates an empty cohort without draft apprenticeship
        /// </summary>
        internal Cohort(long providerId,
            long accountId,
            long accountLegalEntityId,
            Party originatingParty,
            UserInfo userInfo) : this(providerId, accountId, accountLegalEntityId, null, originatingParty, userInfo)
        {
            WithParty = originatingParty;
            EditStatus = originatingParty.ToEditStatus();
            IsDraft = true;

            StartTrackingSession(UserAction.CreateCohort, originatingParty, accountId, providerId, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        /// <summary>
        /// Creates a cohort with a draft apprenticeship
        /// </summary>
        internal Cohort(long providerId,
            long accountId,
            long accountLegalEntityId,
            long? transferSenderId,
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            Party originatingParty,
            UserInfo userInfo) : this(providerId, accountId, accountLegalEntityId, transferSenderId, originatingParty, userInfo)
        {
            CheckDraftApprenticeshipDetails(draftApprenticeshipDetails);
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, false);
            WithParty = originatingParty;
            EditStatus = originatingParty.ToEditStatus();
            IsDraft = true;

            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, originatingParty);
            Apprenticeships.Add(draftApprenticeship);

            Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));

            StartTrackingSession(UserAction.CreateCohort, originatingParty, accountId, providerId, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.TrackInsert(draftApprenticeship);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        /// <summary>
        /// Creates an empty cohort with other party
        /// </summary>
        internal Cohort(long providerId,
            long accountId,
            long accountLegalEntityId,
            long? transferSenderId,
            Party originatingParty,
            string message,
            UserInfo userInfo) : this(providerId, accountId, accountLegalEntityId, transferSenderId, originatingParty, userInfo)
        {
            CheckIsEmployer(originatingParty);
            IsDraft = false;

            WithParty = originatingParty.GetOtherParty();
            EditStatus = originatingParty.GetOtherParty().ToEditStatus();
            LastAction = LastAction.Amend;
            if (message != null)
            {
                AddMessage(message, originatingParty, userInfo);
            }

            Publish(() => new CohortAssignedToProviderEvent(Id, DateTime.UtcNow));

            StartTrackingSession(UserAction.CreateCohortWithOtherParty, originatingParty, accountId, providerId, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        /// <summary>
        /// Creates a Cohort from a Change of Party Request
        /// </summary>
        internal Cohort(long providerId,
            long accountId,
            long accountLegalEntityId,
            Apprenticeship apprenticeship,
            Guid? reservationId,
            ChangeOfPartyRequest changeOfPartyRequest,
            UserInfo userInfo)
            : this(providerId,
            accountId,
            accountLegalEntityId,
            null,
            changeOfPartyRequest.OriginatingParty,
            userInfo)
        {

            ChangeOfPartyRequestId = changeOfPartyRequest.Id;

            Approvals = changeOfPartyRequest.IsPreApproved();

            WithParty = changeOfPartyRequest.OriginatingParty.GetOtherParty();
            IsDraft = false;

            if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider)
            {                  
                TransferSenderId = apprenticeship.Cohort.TransferSenderId;               
            }

            var draftApprenticeship = apprenticeship.CreateCopyForChangeOfParty(changeOfPartyRequest, reservationId);
            Apprenticeships.Add(draftApprenticeship);

            //Retained for backwards-compatibility:
            EditStatus = WithParty.ToEditStatus();
            LastAction = LastAction.Amend;
            CommitmentStatus = CommitmentStatus.Active;
            
            Publish(() => new CohortWithChangeOfPartyCreatedEvent(Id, changeOfPartyRequest.Id, changeOfPartyRequest.OriginatingParty, DateTime.UtcNow, userInfo));

            if (changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer)
            {
                Publish(() => new CohortAssignedToEmployerEvent(Id, DateTime.UtcNow, Party.Provider));
            }
            else
            {
                Publish(() => new CohortAssignedToProviderEvent(Id, DateTime.UtcNow));
            }
            
            Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));
            
            StartTrackingSession(UserAction.CreateCohortWithChangeOfParty, changeOfPartyRequest.OriginatingParty, accountId, providerId, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.TrackInsert(draftApprenticeship);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public virtual long Id { get; set; }
        public string Reference { get; set; }
        public long EmployerAccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long ProviderId { get; set; }
        public CommitmentStatus CommitmentStatus { get; set; }
        public EditStatus EditStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public LastAction LastAction { get; set; }
        public string LastUpdatedByEmployerName { get; set; }
        public string LastUpdatedByEmployerEmail { get; set; }
        public string LastUpdatedByProviderName { get; set; }
        public string LastUpdatedByProviderEmail { get; set; }
        public long? TransferSenderId { get; set; }
        public TransferApprovalStatus? TransferApprovalStatus { get; set; }
        public string TransferApprovalActionedByEmployerName { get; set; }
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
        public Originator Originator { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDraft { get; set; }

        public byte[] RowVersion { get; set; }
        public DateTime LastUpdatedOn { get; set; }

        public virtual ICollection<ApprenticeshipBase> Apprenticeships { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<TransferRequest> TransferRequests { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual AccountLegalEntity AccountLegalEntity { get; set; }

        public virtual Account TransferSender { get; set; }
        public virtual ChangeOfPartyRequest ChangeOfPartyRequest { get; set; }
        public virtual ApprenticeshipEmployerType? ApprenticeshipEmployerTypeOnApproval { get; set; }

        public IEnumerable<DraftApprenticeship> DraftApprenticeships => Apprenticeships.OfType<DraftApprenticeship>();

        public int DraftApprenticeshipCount => DraftApprenticeships.Count();

        public string LastMessage => Messages.OrderByDescending(x => x.Id).FirstOrDefault()?.Text;

        public virtual Party WithParty { get; set; }
        public virtual Party Approvals { get; set; }
        public DateTime? EmployerAndProviderApprovedOn { get; set; }
        public long? ChangeOfPartyRequestId { get; set; }
        public bool IsLinkedToChangeOfPartyRequest => ChangeOfPartyRequestId.HasValue;

        public virtual bool IsApprovedByAllParties => WithParty == Party.None; //todo: use new Approvals flag

        public DraftApprenticeship AddDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party creator, UserInfo userInfo)
        {
            CheckIsWithParty(creator);
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, false);

            StartTrackingSession(UserAction.AddDraftApprenticeship, creator, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, creator);
            Apprenticeships.Add(draftApprenticeship);
            Approvals = Party.None;
            UpdatedBy(creator, userInfo);
            LastUpdatedOn = DateTime.UtcNow;

            ChangeTrackingSession.TrackInsert(draftApprenticeship);
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));
            return draftApprenticeship;
        }

        public virtual void Approve(Party modifyingParty, string message, UserInfo userInfo, DateTime now, bool apprenticeEmailRequired = false)
        {
            CheckIsEmployerOrProviderOrTransferSender(modifyingParty);
            CheckIsWithParty(modifyingParty);
            CheckIsCompleteForParty(modifyingParty, apprenticeEmailRequired);

            StartTrackingSession(UserAction.ApproveCohort, modifyingParty, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            
            switch (modifyingParty)
            {
                case Party.Employer:
                case Party.Provider:
                    {
                        var otherParty = modifyingParty.GetOtherParty();
                        var isApprovedByOtherParty = Approvals.HasFlag(otherParty);

                        IsDraft = false;
                        EditStatus = isApprovedByOtherParty ? EditStatus.Both : otherParty.ToEditStatus();
                        WithParty = GetWithParty(otherParty, isApprovedByOtherParty);
                        if (isApprovedByOtherParty) EmployerAndProviderApprovedOn = DateTime.UtcNow;
                        LastAction = LastAction.Approve;
                        CommitmentStatus = CommitmentStatus.Active; 
                        TransferApprovalStatus = GetTransferApprovalStatus(isApprovedByOtherParty);
                        Approvals |= modifyingParty;
                        AddMessage(message, modifyingParty, userInfo);
                        UpdatedBy(modifyingParty, userInfo);
                        LastUpdatedOn = DateTime.UtcNow;

                        switch (WithParty)
                        {
                            case Party.Employer:
                                Publish(() => new CohortAssignedToEmployerEvent(Id, now, modifyingParty));
                                break;
                            case Party.Provider:
                                Publish(() => new CohortAssignedToProviderEvent(Id, now));
                                break;
                            case Party.TransferSender:
                                Publish(() => new CohortTransferApprovalRequestedEvent(Id, now, modifyingParty));
                                break;
                        }

                        break;
                    }
                case Party.TransferSender:
                    TransferApprovalStatus = Types.TransferApprovalStatus.Approved;
                    TransferApprovalActionedOn = now;
                    Approvals |= modifyingParty;
                    WithParty = Party.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifyingParty));
            }

            if (Approvals.HasFlag(Party.Provider) && modifyingParty == Party.Employer)
            {
                Publish(() => new CohortApprovedByEmployerEvent(Id, now));
            }

            if (IsApprovedByAllParties)
            {
                Publish(() => new CohortFullyApprovedEvent(Id, EmployerAccountId, ProviderId, now, modifyingParty, ChangeOfPartyRequestId, userInfo));

                if (ChangeOfPartyRequestId.HasValue)
                {
                    Publish(() => new CohortWithChangeOfPartyFullyApprovedEvent(Id, ChangeOfPartyRequestId.Value, now, modifyingParty, userInfo));
                }
            }

            ChangeTrackingSession.CompleteTrackingSession();
        }

        private Party GetWithParty(Party otherParty, bool isApprovedByOtherParty)
        {
            if (isApprovedByOtherParty && TransferSenderId.HasValue && ChangeOfPartyRequestId.HasValue)
            {
                return Party.None;
            }

            return isApprovedByOtherParty
                ? TransferSenderId.HasValue ? Party.TransferSender : Party.None
                : otherParty;
        }

        private TransferApprovalStatus? GetTransferApprovalStatus(bool isApprovedByOtherParty)
        {
            if (isApprovedByOtherParty && TransferSenderId.HasValue && ChangeOfPartyRequestId.HasValue)
            {
                return Types.TransferApprovalStatus.Approved;
            }
            return null;
        }

        public virtual void SendToOtherParty(Party modifyingParty, string message, UserInfo userInfo, DateTime now)
        {
            CheckIsEmployerOrProvider(modifyingParty);
            CheckIsWithParty(modifyingParty);

            StartTrackingSession(UserAction.SendCohort, modifyingParty, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            IsDraft = false;
            EditStatus = modifyingParty.GetOtherParty().ToEditStatus();
            WithParty = modifyingParty.GetOtherParty();
            LastAction = LastAction.Amend;
            CommitmentStatus = CommitmentStatus.Active;
            TransferApprovalStatus = null;
            AddMessage(message, modifyingParty, userInfo);
            UpdatedBy(modifyingParty, userInfo);
            LastUpdatedOn = DateTime.UtcNow;

            switch (WithParty)
            {
                case Party.Employer:
                    Publish(() => new CohortAssignedToEmployerEvent(Id, now, modifyingParty));
                    break;
                case Party.Provider:
                    Publish(() => new CohortAssignedToProviderEvent(Id, now));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(EditStatus));
            }
            
            if (Approvals.HasFlag(Party.Provider))
            {
                Publish(() => new ApprovedCohortReturnedToProviderEvent(Id, now));
            }

            if (ChangeOfPartyRequestId.HasValue)
            {
                Publish(() => new CohortWithChangeOfPartyUpdatedEvent(Id, userInfo));
            }

            Approvals = Party.None;
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public void UpdateDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party modifyingParty, UserInfo userInfo)
        {
            CheckIsWithParty(modifyingParty);

            var existingDraftApprenticeship = GetDraftApprenticeship(draftApprenticeshipDetails.Id);

            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, ChangeOfPartyRequestId.HasValue);

            if (ChangeOfPartyRequestId.HasValue)
            {
                existingDraftApprenticeship.ValidateUpdateForChangeOfParty(draftApprenticeshipDetails);
            }

            StartTrackingSession(UserAction.UpdateDraftApprenticeship, modifyingParty, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            ChangeTrackingSession.TrackUpdate(existingDraftApprenticeship);

            if (existingDraftApprenticeship.IsOtherPartyApprovalRequiredForUpdate(draftApprenticeshipDetails))
            {
                Approvals = Party.None;
            }
            existingDraftApprenticeship.Merge(draftApprenticeshipDetails, modifyingParty);

            UpdatedBy(modifyingParty, userInfo);
            LastUpdatedOn = DateTime.UtcNow;
            Publish(() => new DraftApprenticeshipUpdatedEvent(existingDraftApprenticeship.Id, Id, existingDraftApprenticeship.Uln, existingDraftApprenticeship.ReservationId, DateTime.UtcNow));
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public void AddTransferRequest(string jsonSummary, decimal cost, decimal fundingCap, Party lastApprovedByParty)
        {
            CheckThereIsNoPendingTransferRequest();
            var transferRequest = new TransferRequest();
            transferRequest.Status = (byte) Types.TransferApprovalStatus.Pending;
            transferRequest.TrainingCourses = jsonSummary;
            transferRequest.Cost = cost;
            transferRequest.FundingCap = fundingCap;

            TransferRequests.Add(transferRequest);
            TransferApprovalStatus = Types.TransferApprovalStatus.Pending;
            Publish(() => new TransferRequestCreatedEvent(transferRequest.Id, Id, DateTime.UtcNow, lastApprovedByParty));
        }

		public void Delete(Party modifyingParty, UserInfo userInfo)
        {
            CheckIsWithParty(modifyingParty);

            StartTrackingSession(UserAction.DeleteCohort, modifyingParty, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            LastUpdatedOn = DateTime.UtcNow;

            MarkAsDeletedAndEmitCohortDeletedEvent(modifyingParty, userInfo);

            foreach (var draftApprenticeship in DraftApprenticeships.ToArray())
            {
                RemoveDraftApprenticeship(draftApprenticeship);
            }

            ChangeTrackingSession.CompleteTrackingSession();
        }
        public void DeleteDraftApprenticeship(long draftApprenticeshipId, Party modifyingParty, UserInfo userInfo)
        {
            CheckIsWithParty(modifyingParty);

            var draftApprenticeship = DraftApprenticeships.Single(x => x.Id == draftApprenticeshipId);

            StartTrackingSession(UserAction.DeleteDraftApprenticeship, modifyingParty, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            RemoveDraftApprenticeship(draftApprenticeship);

            LastUpdatedOn = DateTime.UtcNow;
            Approvals = Party.None;
            ResetTransferSenderRejection();

            if (!DraftApprenticeships.Any() && !(modifyingParty == Party.Provider))
            {
                MarkAsDeletedAndEmitCohortDeletedEvent(modifyingParty, userInfo);
            }
            
            ChangeTrackingSession.CompleteTrackingSession();
        }
		
		private void RemoveDraftApprenticeship(DraftApprenticeship draftApprenticeship)
        {
            ChangeTrackingSession.TrackDelete(draftApprenticeship);
            LastUpdatedOn = DateTime.UtcNow;
            Apprenticeships.Remove(draftApprenticeship);
            Publish(() => new DraftApprenticeshipDeletedEvent
            {
                DraftApprenticeshipId = draftApprenticeship.Id,
                CohortId = draftApprenticeship.CommitmentId,
                Uln = draftApprenticeship.Uln,
                ReservationId = draftApprenticeship.ReservationId,
                DeletedOn = DateTime.UtcNow
            });
        }
		
		private void MarkAsDeletedAndEmitCohortDeletedEvent(Party deletedBy, UserInfo userInfo)
        {
            var approvalStatusPriorToDeletion = Approvals;
            IsDeleted = true;
            Publish(() => new CohortDeletedEvent(Id, EmployerAccountId, ProviderId, approvalStatusPriorToDeletion, DateTime.UtcNow));

            if (ChangeOfPartyRequestId.HasValue)
            {
                Publish(() => new CohortWithChangeOfPartyDeletedEvent(Id, ChangeOfPartyRequestId.Value, DateTime.UtcNow, deletedBy, userInfo));
            }
        }

        private void ResetTransferSenderRejection()
        {
            if (TransferApprovalStatus == Types.TransferApprovalStatus.Rejected)
            {
                TransferApprovalStatus = null;
                TransferApprovalActionedOn = null;
                LastAction = LastAction.AmendAfterRejected;
            }
        }

        private void CheckThereIsNoPendingTransferRequest()
        {
            if (TransferRequests.Any(x =>x.Status == (byte) Types.TransferApprovalStatus.Pending))
            {
                throw new DomainException(nameof(TransferRequests), $"Cohort already has a pending transfer request");
            }
        }

        private void AddMessage(string text, Party sendingParty, UserInfo userInfo)
        {
            Messages.Add(new Message(this, sendingParty, userInfo.UserDisplayName, text ?? ""));
        }

        private void ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails, bool isContinuation)
        {
            var errors = new List<DomainError>();
            errors.AddRange(BuildEndDateValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildCostValidationFailures(draftApprenticeshipDetails));
            if (!isContinuation)
            {
                errors.AddRange(BuildFirstNameValidationFailures(draftApprenticeshipDetails));
                errors.AddRange(BuildLastNameValidationFailures(draftApprenticeshipDetails));
                errors.AddRange(BuildEmailValidationFailures(draftApprenticeshipDetails));
                errors.AddRange(BuildStartDateValidationFailures(draftApprenticeshipDetails));
                errors.AddRange(BuildDateOfBirthValidationFailures(draftApprenticeshipDetails));
                errors.AddRange(BuildUlnValidationFailures(draftApprenticeshipDetails));
                errors.AddRange(BuildTrainingProgramValidationFailures(draftApprenticeshipDetails));
            }
            errors.ThrowIfAny();
        }

        private IEnumerable<DomainError> BuildFirstNameValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.FirstName))
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.FirstName), "First name must be entered");
            }
        }

        private IEnumerable<DomainError> BuildLastNameValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.LastName))
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.LastName), "Last name must be entered");
            }
        }

        private IEnumerable<DomainError> BuildEmailValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (draftApprenticeshipDetails.Email != null)
            {
                if (!draftApprenticeshipDetails.Email.IsAValidEmailAddress())
                {
                    yield return new DomainError(nameof(draftApprenticeshipDetails.Email), "Please enter a valid email address");
                }
            }
        }

        private IEnumerable<DomainError> BuildEndDateValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (draftApprenticeshipDetails.EndDate.HasValue && draftApprenticeshipDetails.EndDate < Constants.DasStartDate)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The end date must not be earlier than May 2017");
                yield break;
            }

            if (draftApprenticeshipDetails.EndDate.HasValue && draftApprenticeshipDetails.StartDate.HasValue && draftApprenticeshipDetails.EndDate <= draftApprenticeshipDetails.StartDate)
            {
                    yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The end date must not be on or before the start date");
            }
        }

        private IEnumerable<DomainError> BuildCostValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (draftApprenticeshipDetails.Cost.HasValue && draftApprenticeshipDetails.Cost <= 0)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "Enter the total agreed training cost");
                yield break;
            }

            if (draftApprenticeshipDetails.Cost.HasValue && draftApprenticeshipDetails.Cost > Constants.MaximumApprenticeshipCost)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "The total cost must be £100,000 or less");
            }
        }

        private IEnumerable<DomainError> BuildDateOfBirthValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (draftApprenticeshipDetails.AgeOnStartDate.HasValue && draftApprenticeshipDetails.AgeOnStartDate.Value < Constants.MinimumAgeAtApprenticeshipStart)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The apprentice must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training");
                yield break;
            }

            if (draftApprenticeshipDetails.AgeOnStartDate.HasValue && draftApprenticeshipDetails.AgeOnStartDate >= Constants.MaximumAgeAtApprenticeshipStart)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The apprentice must be younger than {Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training");
                yield break;
            }

            if (draftApprenticeshipDetails.DateOfBirth.HasValue &&  draftApprenticeshipDetails.DateOfBirth < Constants.MinimumDateOfBirth)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The Date of birth is not valid");
            }
        }

        private IEnumerable<DomainError> BuildTrainingProgramValidationFailures(DraftApprenticeshipDetails details)
        {
            if (details.TrainingProgramme == null) yield break;

            if (details.TrainingProgramme?.ProgrammeType == ProgrammeType.Framework && TransferSenderId.HasValue)
            {
                yield return new DomainError(nameof(details.TrainingProgramme.CourseCode), "Entered course is not valid.");
            }
        }

        private IEnumerable<DomainError> BuildStartDateValidationFailures(DraftApprenticeshipDetails details)
        {
            if (!details.StartDate.HasValue) yield break;

            var courseStartedBeforeDas = details.TrainingProgramme != null &&
                                         (!details.TrainingProgramme.EffectiveFrom.HasValue ||
                                          details.TrainingProgramme.EffectiveFrom.Value < Constants.DasStartDate);

            var trainingProgrammeStatus = details.TrainingProgramme?.GetStatusOn(details.StartDate.Value);
            
            if((details.StartDate.Value < Constants.DasStartDate) && (!trainingProgrammeStatus.HasValue || courseStartedBeforeDas))
            {
                yield return new DomainError(nameof(details.StartDate), "The start date must not be earlier than May 2017");
                yield break;
            }

            if (trainingProgrammeStatus.HasValue && trainingProgrammeStatus.Value != TrainingProgrammeStatus.Active)
            {
                var suffix = trainingProgrammeStatus == TrainingProgrammeStatus.Pending
                    ? $"after {details.TrainingProgramme.EffectiveFrom.Value.AddMonths(-1):MM yyyy}"
                    : $"before {details.TrainingProgramme.EffectiveTo.Value.AddMonths(1):MM yyyy}";

                var errorMessage = $"This training course is only available to apprentices with a start date {suffix}";

                yield return new DomainError(nameof(details.StartDate), errorMessage);
                yield break;
            }

            if (trainingProgrammeStatus.HasValue && TransferSenderId.HasValue
                && details.StartDate.Value < Constants.TransferFeatureStartDate)
            {
                var errorMessage = $"Apprentices funded through a transfer can't start earlier than May 2018";

                yield return new DomainError(nameof(details.StartDate), errorMessage);
            }
        }

        private IEnumerable<DomainError> BuildUlnValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln))
            {
                yield break;
            }
            
            if (Apprenticeships.Any(a => a.Id != draftApprenticeshipDetails.Id && a.Uln == draftApprenticeshipDetails.Uln))
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Uln), "The unique learner number has already been used for an apprentice in this cohort");
            }
        }

        private void CheckIsWithParty(Party party)
        {
            if (party != WithParty)
            {
                throw new DomainException(nameof(WithParty), $"Cohort must be with the party; {party} is not valid");
            }
        }

        private void CheckIsCompleteForParty(Party party, bool apprenticeEmailRequired)
        {
            if (!DraftApprenticeships.Any())
            {
                throw new DomainException(nameof(Apprenticeships), $"Cohort must have at least one draft apprenticeship");
            }

            if (party == Party.Employer || party == Party.Provider)
            {
                if (DraftApprenticeships.Any(x => !x.IsCompleteForParty(party, apprenticeEmailRequired)))
                {
                    throw new DomainException(nameof(DraftApprenticeships), $"Cohort must be complete for {party}");
                }
            }
        }

        private void CheckIsEmployer(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"Party must be {Party.Employer}; {party} is not valid");
            }
        }

        private void CheckIsEmployerOrProvider(Party party)
        {
            if (party != Party.Employer && party != Party.Provider)
            {
                throw new DomainException(nameof(party), $"Party must be {Party.Employer} or {Party.Provider}; {party} is not valid");
            }
        }

        private void CheckIsEmployerOrProviderOrTransferSender(Party party)
        {
            if (party != Party.Employer && party != Party.Provider && party != Party.TransferSender)
            {
                throw new DomainException(nameof(party), $"Party must be {Party.Employer} or {Party.Provider} or {Party.TransferSender}; {party} is not valid");
            }
        }

        private void CheckDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (draftApprenticeshipDetails == null)
            {
                throw new DomainException(nameof(draftApprenticeshipDetails), "DraftApprenticeshipDetails must be supplied");
            }
        }

        private void UpdatedBy(Party party, UserInfo userInfo)
        {
            if (userInfo == null)
            {
                return;
            }

            switch (party)
            {
                case Party.Employer:
                    LastUpdatedByEmployerName = userInfo.UserDisplayName;
                    LastUpdatedByEmployerEmail = userInfo.UserEmail;
                    break;
                case Party.Provider:
                    LastUpdatedByProviderName = userInfo.UserDisplayName;
                    LastUpdatedByProviderEmail = userInfo.UserEmail;
                    break;
            }
        }

        public void RejectTransferRequest(UserInfo userInfo)
        {
            CheckIsWithParty(Party.TransferSender);
            StartTrackingSession(UserAction.RejectTransferRequest, Party.TransferSender, EmployerAccountId, ProviderId, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            EditStatus = EditStatus.EmployerOnly;
            WithParty = Party.Employer;
            LastUpdatedOn = DateTime.UtcNow;
            ChangeTrackingSession.CompleteTrackingSession();
        }

        internal DraftApprenticeship GetDraftApprenticeship(long draftApprenticeshipId)
        {
            var existingDraftApprenticeship = DraftApprenticeships.SingleOrDefault(a => a.Id == draftApprenticeshipId);

            if (existingDraftApprenticeship == null)
            {
                throw new InvalidOperationException($"There is not a draft apprenticeship with id {draftApprenticeshipId} in cohort {Id}");
            }

            return existingDraftApprenticeship;
        }
    }
}