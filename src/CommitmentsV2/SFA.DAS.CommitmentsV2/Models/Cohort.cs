using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using TrainingProgrammeStatus = SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Cohort : Entity
    {
        public Cohort()
        {
            Apprenticeships = new HashSet<Apprenticeship>();
            Messages = new HashSet<Message>();
            TransferRequests = new HashSet<TransferRequest>();
        }

        internal Cohort(Provider provider, AccountLegalEntity accountLegalEntity, Party originatingParty) : this()
        {
            CheckOriginatorIsEmployerOrProvider(originatingParty);

            EmployerAccountId = accountLegalEntity.AccountId;
            LegalEntityId = accountLegalEntity.LegalEntityId;
            LegalEntityName = accountLegalEntity.Name;
            LegalEntityAddress = accountLegalEntity.Address;
            LegalEntityOrganisationType = accountLegalEntity.OrganisationType;
            AccountLegalEntityPublicHashedId = accountLegalEntity.PublicHashedId;
            ProviderId = provider.UkPrn;
            ProviderName = provider.Name;

            // Reference cannot be set until we've saved the commitment (as we need the Id) but it's non-nullable so we'll use a temp value
            Reference = "";
            Originator = originatingParty.ToOriginator();
            CommitmentStatus = CommitmentStatus.New;
            CreatedOn = DateTime.UtcNow;
        }

        //constructor for creating cohort with self
        internal Cohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            DraftApprenticeshipDetails draftApprenticeshipDetails,
            Party originatingParty,
            UserInfo userInfo) : this(provider, accountLegalEntity, originatingParty)
        {
            CheckOriginatorIsEmployerOrProvider(originatingParty);
            CheckDraftApprenticeshipDetails(draftApprenticeshipDetails);

            EditStatus = originatingParty.ToEditStatus();
            LastAction = LastAction.None;
            AddDraftApprenticeship(draftApprenticeshipDetails, originatingParty, userInfo);
        }

        //constructor for creating cohort with other party
        internal Cohort(Provider provider,
            AccountLegalEntity accountLegalEntity,
            Party originatingParty,
            string message,
            UserInfo userInfo) : this(provider, accountLegalEntity, originatingParty)
        {
            CheckOriginatorIsEmployer(originatingParty);

            EditStatus = originatingParty.GetOtherParty().ToEditStatus();
            LastAction = LastAction.Amend;
            AddMessage(message, originatingParty, userInfo);
        }

        public virtual long Id { get; set; }
        public string Reference { get; set; }
        public long EmployerAccountId { get; set; }
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
        public byte? TransferApprovalStatus { get; set; }
        public string TransferApprovalActionedByEmployerName { get; set; }
        public string TransferApprovalActionedByEmployerEmail { get; set; }
        public DateTime? TransferApprovalActionedOn { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public Originator Originator { get; set; }

        public virtual ICollection<Apprenticeship> Apprenticeships { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<TransferRequest> TransferRequests { get; set; }

        public IEnumerable<DraftApprenticeship> DraftApprenticeships => Apprenticeships.OfType<DraftApprenticeship>();

        public DraftApprenticeship AddDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party creator, UserInfo userInfo)
        {
            EnsureModifierIsAllowedToModifyDraftApprenticeship(creator);
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);
            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, creator);
            Apprenticeships.Add(draftApprenticeship);
            UpdatedBy(userInfo, creator);
            Publish(() => new DraftApprenticeshipCreatedEvent(draftApprenticeship.Id, Id, draftApprenticeship.Uln, draftApprenticeship.ReservationId, draftApprenticeship.CreatedOn.Value));
            return draftApprenticeship;
        }

        public void UpdateDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, Party modifyingParty, UserInfo userInfo)
        {
            EnsureModifierIsAllowedToModifyDraftApprenticeship(modifyingParty);

            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);
            var existingDraftApprenticeship = DraftApprenticeships.SingleOrDefault(a => a.Id == draftApprenticeshipDetails.Id);

            if (existingDraftApprenticeship == null)
            {
                throw new InvalidOperationException($"There is not a draft apprenticeship with id {draftApprenticeshipDetails.Id} in cohort {Id}");
            }
            
            existingDraftApprenticeship.Merge(draftApprenticeshipDetails, modifyingParty);
            UpdatedBy(userInfo, modifyingParty);
            Publish(() => new DraftApprenticeshipUpdatedEvent(existingDraftApprenticeship.Id, Id, existingDraftApprenticeship.Uln, existingDraftApprenticeship.ReservationId, DateTime.UtcNow));
        }

        private void AddMessage(string text, Party sendingParty, UserInfo userInfo)
        {
            Messages.Add(new Message(this, sendingParty, userInfo.UserDisplayName, text));
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
            errors.ThrowIfAny();
        }
        

        private void EnsureModifierIsAllowedToModifyDraftApprenticeship(Party modifyingParty)
        {
            if (!ModifierIsAllowedToEdit(modifyingParty))
            {
                throw new DomainException(nameof(modifyingParty), "The cohort may not be modified by the current role");
            }
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
            if (draftApprenticeshipDetails.EndDate.HasValue && draftApprenticeshipDetails.EndDate <= DateTime.Today)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The end date must not be in the past");
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

        private void CheckOriginatorIsEmployerOrProvider(Party originator)
        {
            if (originator != Party.Employer && originator != Party.Provider)
            {
                throw new DomainException("Creator", $"Cohorts can only be created by Employer or Provider; {originator} is not valid");
            }
        }


        private void CheckOriginatorIsEmployer(Party originator)
        {
            if (originator != Party.Employer)
            {
                throw new DomainException("Creator", $"Cohorts can only be created with the other party by the Employer; {originator} is not valid");
            }
        }

        private void CheckDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (draftApprenticeshipDetails == null)
            {
                throw new DomainException("DraftApprenticeshipDetails", "DraftApprenticeshipDetails must be supplied");
            }
        }

        private bool ModifierIsAllowedToEdit(Party modifyingParty)
        {
            if (EditStatus == EditStatus.EmployerOnly && modifyingParty == Party.Employer)
            {
                return true;
            }

            if (EditStatus == EditStatus.ProviderOnly && modifyingParty == Party.Provider)
            {
                return true;
            }

            //Employers can modify Provider-assigned Cohorts during their initial creation
            if (EditStatus == EditStatus.ProviderOnly && modifyingParty == Party.Employer && LastAction == LastAction.None)
            {
                return true;
            }

            return false;
        }

        private void UpdatedBy(UserInfo userInfo, Party modifyingParty)
        {
            if (userInfo == null)
                return;

            switch (modifyingParty)
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
    }
}
