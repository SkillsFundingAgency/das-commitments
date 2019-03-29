using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Models
{
    public partial class Commitment
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

        public virtual void AddDraftApprenticeship(DraftApprenticeshipDetails draftApprenticeshipDetails, IUlnValidator ulnValidator)
        {
            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails, ulnValidator);
            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails);
            Apprenticeship.Add(draftApprenticeship);
        }

        private void ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails, IUlnValidator ulnValidator)
        {
            var errors = new List<DomainError>();
            errors.AddRange(BuildNameValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildEndDateValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildCostValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildReferenceValidationFailures(draftApprenticeshipDetails));
                
            errors.ThrowIfAny();
        }

        private IEnumerable<DomainError> BuildNameValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {

            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.FirstName))
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.FirstName), "First name must be entered");
            }
            else
            {
                if (draftApprenticeshipDetails.FirstName.Length > 100)
                {
                    yield return new DomainError(nameof(draftApprenticeshipDetails.FirstName), "You must enter a first name that's no longer than 100 characters");
                }
            }

            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.LastName))
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.LastName), "Last name must be entered");
            }
            else
            {
                if (draftApprenticeshipDetails.LastName.Length > 100)
                {
                    yield return new DomainError(nameof(draftApprenticeshipDetails.LastName), "You must enter a last name that's no longer than 100 characters");
                }
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
            if (draftApprenticeshipDetails.Cost.HasValue && !(draftApprenticeshipDetails.Cost >= 0 && draftApprenticeshipDetails.Cost <= 100000))
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "The total cost must be £100,000 or less");
            }
        }

        private IEnumerable<DomainError> BuildReferenceValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (EditStatus == EditStatus.ProviderOnly)
            {
                if (draftApprenticeshipDetails.ProviderRef != null && draftApprenticeshipDetails.ProviderRef.Length > 20)
                {
                    yield return new DomainError(nameof(draftApprenticeshipDetails.ProviderRef), "The Reference must be 20 characters or fewer");
                }
            }
            else
            {
                if (draftApprenticeshipDetails.EmployerRef != null && draftApprenticeshipDetails.EmployerRef.Length > 20)
                {
                    yield return new DomainError(nameof(draftApprenticeshipDetails.EmployerRef), "The Reference must be 20 characters or fewer");
                }
            }
        }
    }
}
