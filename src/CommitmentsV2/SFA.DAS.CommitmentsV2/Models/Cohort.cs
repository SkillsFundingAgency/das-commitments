using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Extensions;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using TrainingProgrammeStatus = SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Cohort : Aggregate, ITrackableEntity
    {
        public Cohort()
        {
            Apprenticeships = new HashSet<ApprenticeshipBase>();
            Messages = new HashSet<Message>();
            TransferRequests = new HashSet<TransferRequest>();
        }

        private Cohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            Account transferSender,
            Party originatingParty, UserInfo userInfo) : this()
        {
            CheckIsEmployerOrProvider(originatingParty);

            EmployerAccountId = accountLegalEntity.AccountId;
            AccountLegalEntityId = accountLegalEntity.Id;
            ProviderId = provider.UkPrn;
            TransferSenderId = transferSender?.Id;
            IsDraft = true;

            //Setting of these fields is here for backwards-compatibility only
            ProviderName = provider.Name;
            TransferSenderName = transferSender?.Name;
            LegalEntityId = accountLegalEntity.LegalEntityId;
            LegalEntityName = accountLegalEntity.Name;
            LegalEntityAddress = accountLegalEntity.Address;
            LegalEntityOrganisationType = accountLegalEntity.OrganisationType;
            AccountLegalEntityPublicHashedId = accountLegalEntity.PublicHashedId;

            // Reference cannot be set until we've saved the commitment (as we need the Id) but it's non-nullable so we'll use a temp value
            Reference = "";
            Originator = originatingParty.ToOriginator();
            UpdatedBy(originatingParty, userInfo);
            CommitmentStatus = CommitmentStatus.New;
            CreatedOn = DateTime.UtcNow;
            LastAction = LastAction.None;
        }

        /// <summary>
        /// Creates an empty cohort without draft apprenticeship
        /// </summary>
        internal Cohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            Party originatingParty,
            UserInfo userInfo) : this(provider, accountLegalEntity, null, originatingParty, userInfo)
        {
            EditStatus = originatingParty.ToEditStatus();
            IsDraft = true;

            StartTrackingSession(UserAction.CreateCohort, originatingParty, accountLegalEntity.AccountId, provider.UkPrn, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        /// <summary>
        /// Creates a cohort with a draft apprenticeship
        /// </summary>
        internal Cohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            Account transferSender,
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            Party originatingParty,
            UserInfo userInfo) : this(provider, accountLegalEntity, transferSender, originatingParty, userInfo)
        {
            CheckDraftApprenticeshipDetails(draftApprenticeshipDetails);
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);
            EditStatus = originatingParty.ToEditStatus();
            IsDraft = true;

            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, originatingParty);
            Apprenticeships.Add(draftApprenticeship);

            Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));

            StartTrackingSession(UserAction.CreateCohort, originatingParty, accountLegalEntity.AccountId, provider.UkPrn, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.TrackInsert(draftApprenticeship);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        /// <summary>
        /// Creates an empty cohort with other party
        /// </summary>
        internal Cohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            Account transferSender,
            Party originatingParty,
            string message,
            UserInfo userInfo) : this(provider, accountLegalEntity, transferSender, originatingParty, userInfo)
        {
            CheckIsEmployer(originatingParty);
            IsDraft = false;

            EditStatus = originatingParty.GetOtherParty().ToEditStatus();
            LastAction = LastAction.Amend;
            if (message != null)
            {
                AddMessage(message, originatingParty, userInfo);
            }

            StartTrackingSession(UserAction.CreateCohort, originatingParty, accountLegalEntity.AccountId, provider.UkPrn, userInfo);
            ChangeTrackingSession.TrackInsert(this);
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public virtual long Id { get; set; }
        public string Reference { get; set; }
        public long EmployerAccountId { get; set; }
        public long? AccountLegalEntityId { get; set; }
        public string LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string LegalEntityAddress { get; set; }
        public OrganisationType LegalEntityOrganisationType { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public CommitmentStatus CommitmentStatus { get; set; }
        public EditStatus EditStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public LastAction LastAction { get; set; }
        public string LastUpdatedByEmployerName { get; set; }
        public string LastUpdatedByEmployerEmail { get; set; }
        public string LastUpdatedByProviderName { get; set; }
        public string LastUpdatedByProviderEmail { get; set; }
        public long? TransferSenderId { get; set; }
        public string TransferSenderName { get; set; }
        public TransferApprovalStatus? TransferApprovalStatus { get; set; }
        public string TransferApprovalActionedByEmployerName { get; set; }
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public Originator Originator { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDraft { get; set; }

        public virtual ICollection<ApprenticeshipBase> Apprenticeships { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<TransferRequest> TransferRequests { get; set; }

        public virtual AccountLegalEntity AccountLegalEntity { get; set; }
        public virtual Provider Provider { get; set; }
        public virtual Account TransferSender { get; set; }

        public IEnumerable<DraftApprenticeship> DraftApprenticeships => Apprenticeships.OfType<DraftApprenticeship>();

        public int DraftApprenticeshipCount => DraftApprenticeships.Count();

        public string LastMessage => Messages.OrderByDescending(x => x.Id).FirstOrDefault()?.Text;

        public Party WithParty
        {
            get
            {
                switch (EditStatus)
                {
                    case EditStatus.EmployerOnly:
                        return Party.Employer;
                    case EditStatus.ProviderOnly:
                        return Party.Provider;
                    case EditStatus.Both when TransferSenderId != null && TransferApprovalStatus != Types.TransferApprovalStatus.Approved:
                        return Party.TransferSender;
                    default:
                        return Party.None;
                }
            }
        }
        
        public virtual bool IsApprovedByAllParties => EditStatus == EditStatus.Both && (TransferSenderId == null || TransferApprovalStatus == Types.TransferApprovalStatus.Approved);

        public DraftApprenticeship AddDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party creator, UserInfo userInfo)
        {
            CheckIsWithParty(creator);
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);

            StartTrackingSession(UserAction.AddDraftApprenticeship, creator, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, creator);
            Apprenticeships.Add(draftApprenticeship);
            ResetApprovals();
            UpdatedBy(creator, userInfo);

            ChangeTrackingSession.TrackInsert(draftApprenticeship);
            ChangeTrackingSession.CompleteTrackingSession();

            Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));
            return draftApprenticeship;
        }

        public virtual void Approve(Party modifyingParty, string message, UserInfo userInfo, DateTime now)
        {
            CheckIsEmployerOrProviderOrTransferSender(modifyingParty);
            CheckIsWithParty(modifyingParty);
            CheckHasDraftApprenticeships();

            StartTrackingSession(UserAction.ApproveCohort, modifyingParty, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            switch (modifyingParty)
            {
                case Party.Employer:
                case Party.Provider:
                {
                    var otherParty = modifyingParty.GetOtherParty();
                    var isApprovedByOtherParty = IsApprovedByParty(otherParty);

                    IsDraft = false;
                    EditStatus = isApprovedByOtherParty ? EditStatus.Both : otherParty.ToEditStatus();
                    LastAction = LastAction.Approve;
                    CommitmentStatus = CommitmentStatus.Active;
                    TransferApprovalStatus = null;
                    DraftApprenticeships.ForEach(a => a.Approve(modifyingParty, now));
                    AddMessage(message, modifyingParty, userInfo);
                    UpdatedBy(modifyingParty, userInfo);

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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(modifyingParty));
            }

            if (IsApprovedByParty(Party.Provider) && modifyingParty == Party.Employer)
            {
                Publish(() => new CohortApprovedByEmployerEvent(Id, now));
            }

            if (IsApprovedByAllParties)
            {
                Publish(() => new CohortFullyApprovedEvent(Id, EmployerAccountId, ProviderId.Value, now, modifyingParty));
            }

            ChangeTrackingSession.CompleteTrackingSession();
        }

        public virtual void SendToOtherParty(Party modifyingParty, string message, UserInfo userInfo, DateTime now)
        {
            CheckIsEmployerOrProvider(modifyingParty);
            CheckIsWithParty(modifyingParty);

            StartTrackingSession(UserAction.SendCohort, modifyingParty, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);

            IsDraft = false;
            EditStatus = modifyingParty.GetOtherParty().ToEditStatus();
            LastAction = LastAction.Amend;
            CommitmentStatus = CommitmentStatus.Active;
            TransferApprovalStatus = null;
            AddMessage(message, modifyingParty, userInfo);
            UpdatedBy(modifyingParty, userInfo);

            switch (EditStatus)
            {
                case EditStatus.EmployerOnly:
                    Publish(() => new CohortAssignedToEmployerEvent(Id, now, modifyingParty));
                    break;
                case EditStatus.ProviderOnly:
                    Publish(() => new CohortAssignedToProviderEvent(Id, now));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(EditStatus));
            }
            
            if (IsApprovedByParty(Party.Provider))
            {
                Publish(() => new ApprovedCohortReturnedToProviderEvent(Id, now));
            }
            
            ResetApprovals();
            ChangeTrackingSession.CompleteTrackingSession();
        }

        public void UpdateDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party modifyingParty, UserInfo userInfo)
        {
            CheckIsWithParty(modifyingParty);
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);

            var existingDraftApprenticeship = DraftApprenticeships.SingleOrDefault(a => a.Id == draftApprenticeshipDetails.Id);

            if (existingDraftApprenticeship == null)
            {
                throw new InvalidOperationException($"There is not a draft apprenticeship with id {draftApprenticeshipDetails.Id} in cohort {Id}");
            }

            StartTrackingSession(UserAction.UpdateDraftApprenticeship, modifyingParty, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            ChangeTrackingSession.TrackUpdate(existingDraftApprenticeship);

            existingDraftApprenticeship.Merge(draftApprenticeshipDetails, modifyingParty);
            if (existingDraftApprenticeship.AgreementStatus == AgreementStatus.NotAgreed)
            {
                ResetApprovals();
            }

            UpdatedBy(modifyingParty, userInfo);
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

            StartTrackingSession(UserAction.DeleteCohort, modifyingParty, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            
            MarkAsDeletedAndEmitCohortDeletedEvent();

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

            StartTrackingSession(UserAction.DeleteDraftApprenticeship, modifyingParty, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            ChangeTrackingSession.TrackDelete(draftApprenticeship);

            RemoveDraftApprenticeship(draftApprenticeship);

            ResetApprovals();
            ResetTransferSenderRejection();

            if (!DraftApprenticeships.Any())
            {
                MarkAsDeletedAndEmitCohortDeletedEvent();
            }
            
            ChangeTrackingSession.CompleteTrackingSession();
        }
		
		private void RemoveDraftApprenticeship(DraftApprenticeship draftApprenticeship)
        {
            ChangeTrackingSession.TrackDelete(draftApprenticeship);
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
		
		private void MarkAsDeletedAndEmitCohortDeletedEvent()
        {
            var approvalStatusPriorToDeletion = Approvals;
            IsDeleted = true;
            Publish(() => new CohortDeletedEvent(Id, EmployerAccountId, ProviderId.Value, approvalStatusPriorToDeletion, DateTime.UtcNow));
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
            Messages.Add(new Message(this, sendingParty, userInfo.UserDisplayName, text == null || EditStatus == EditStatus.Both ? "" : text));
        }

        private void ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            var errors = new List<DomainError>();
            errors.AddRange(BuildFirstNameValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildLastNameValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildEndDateValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildCostValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildDateOfBirthValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildStartDateValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildUlnValidationFailures(draftApprenticeshipDetails));
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

        private void CheckHasDraftApprenticeships()
        {
            if (!DraftApprenticeships.Any())
            {
                throw new DomainException(nameof(Apprenticeships), $"Cohort must have at least one draft apprenticeship");
            }
        }

        private void CheckIsWithParty(Party party)
        {
            // Employers can modify Provider-assigned cohorts during their initial creation
            if (party == Party.Employer && EditStatus == EditStatus.ProviderOnly && LastAction == LastAction.None)
            {
                return;
            }
            
            if (party != WithParty)
            {
                throw new DomainException(nameof(party), $"Cohort must be with the party; {party} is not valid");
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

        private bool IsApprovedByParty(Party party)
        {
            switch (party)
            {
                case Party.Employer:
                    return Apprenticeships.Count > 0 &&
                           Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.EmployerAgreed) ||
                           EditStatus == EditStatus.Both;
                case Party.Provider:
                    return Apprenticeships.Count > 0 &&
                           Apprenticeships.All(a => a.AgreementStatus == AgreementStatus.ProviderAgreed) ||
                           EditStatus == EditStatus.Both;
                case Party.TransferSender:
                    return TransferSenderId != null &&
                           TransferApprovalStatus == Types.TransferApprovalStatus.Approved;
                default:
                    throw new ArgumentOutOfRangeException(nameof(party));
            }
        }

        private void ResetApprovals()
        {
            foreach (var apprenticeship in Apprenticeships)
            {
                apprenticeship.AgreementStatus = AgreementStatus.NotAgreed;
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

        public Party Approvals
        {
            get
            {
                var approvals = Party.None;
                if (IsApprovedByParty(Party.Employer))
                {
                    approvals |= Party.Employer;
                }
                if (IsApprovedByParty(Party.Provider))
                {
                    approvals |= Party.Provider;
                }
                if (IsApprovedByParty(Party.TransferSender))
                {
                    approvals |= Party.TransferSender;
                }

                return approvals;
            }
        }

        public void RejectTransferRequest(UserInfo userInfo)
        {
            CheckIsWithParty(Party.TransferSender);
            StartTrackingSession(UserAction.RejectTransferRequest, Party.TransferSender, EmployerAccountId, ProviderId.Value, userInfo);
            ChangeTrackingSession.TrackUpdate(this);
            EditStatus = EditStatus.EmployerOnly;
            ChangeTrackingSession.CompleteTrackingSession();
        }
    }
}