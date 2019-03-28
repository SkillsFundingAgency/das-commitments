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

            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.FirstName))
            {
                errors.Add(new DomainError(nameof(draftApprenticeshipDetails.FirstName), "First name is required"));
            }

            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.LastName))
            {
                errors.Add(new DomainError(nameof(draftApprenticeshipDetails.LastName), "Last name is required"));
            }

            if (!string.IsNullOrEmpty(draftApprenticeshipDetails.Uln))
            {
                switch (ulnValidator.Validate(draftApprenticeshipDetails.Uln))
                {
                    case UlnValidationResult.IsEmptyUlnNumber:
                        errors.Add(new DomainError(nameof(draftApprenticeshipDetails.Uln), "You must enter a Uln that is not empty"));
                        break;
                    case UlnValidationResult.IsInValidTenDigitUlnNumber:
                        errors.Add(new DomainError(nameof(draftApprenticeshipDetails.Uln), "You must enter a Uln that is 10 digits long"));
                        break;
                    case UlnValidationResult.IsInvalidUln:
                        errors.Add(new DomainError(nameof(draftApprenticeshipDetails.Uln), "You must enter a valid Uln"));
                        break;
                }

            }
            //todo...more rules here...

            errors.ThrowIfAny();
        }
    }
}
