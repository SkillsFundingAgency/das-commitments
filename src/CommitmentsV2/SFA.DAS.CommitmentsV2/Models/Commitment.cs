using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Services;
using TrainingProgrammeStatus = SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Commitment
    {
        public Commitment()
        {
            Apprenticeship = new HashSet<Apprenticeship>();
            Message = new HashSet<Message>();
            TransferRequest = new HashSet<TransferRequest>();
        }

        public long Id { get; set; }
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

        public virtual ICollection<Apprenticeship> Apprenticeship { get; set; }
        public virtual ICollection<Message> Message { get; set; }
        public virtual ICollection<TransferRequest> TransferRequest { get; set; }

        public virtual void AddDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);
            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, Originator);
            Apprenticeship.Add(draftApprenticeship);
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

            if (draftApprenticeshipDetails.Cost.HasValue && draftApprenticeshipDetails.Cost > 100000)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "The total cost must be £100,000 or less");
            }
        }


        private IEnumerable<DomainError> BuildDateOfBirthValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (!draftApprenticeshipDetails.AgeOnStartDate.HasValue) yield break;

            if (draftApprenticeshipDetails.AgeOnStartDate < 15)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), "The apprentice must be at least 15 years old at the start of their training");
            }
            else if (draftApprenticeshipDetails.AgeOnStartDate >= 115)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), "The apprentice must be younger than 115 years old at the start of their training");
            }
        }

        private IEnumerable<DomainError> BuildStartDateValidationFailures(DraftApprenticeshipDetails details)
        {
            if (!details.StartDate.HasValue) yield break;

            var dasStartDate = new DateTime(2017, 5, 1, 0, 0, 0, DateTimeKind.Utc);

            var courseStartedBeforeDas = details.TrainingProgramme != null &&
                                         (!details.TrainingProgramme.EffectiveFrom.HasValue ||
                                          details.TrainingProgramme.EffectiveFrom.Value < dasStartDate);

            var trainingProgrammeStatus = details.TrainingProgramme?.GetStatusOn(details.StartDate.Value);
            
            if((details.StartDate.Value < dasStartDate) && (!trainingProgrammeStatus.HasValue || courseStartedBeforeDas))
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
    }
}
