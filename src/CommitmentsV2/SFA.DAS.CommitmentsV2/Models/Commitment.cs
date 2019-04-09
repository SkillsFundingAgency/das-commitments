using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Validation;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

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

        public async Task AddDraftApprenticeshipAsync(DraftApprenticeshipDetails draftApprenticeshipDetails,
            IDomainValidator domainValidator)
        {
            await ValidateDraftApprenticeshipDetailsAsync(draftApprenticeshipDetails, domainValidator);
            var draftApprenticeship = new DraftApprenticeship(draftApprenticeshipDetails, Originator);
            Apprenticeship.Add(draftApprenticeship);
        }

        private async Task ValidateDraftApprenticeshipDetailsAsync(DraftApprenticeshipDetails draftApprenticeshipDetails, IDomainValidator domainValidator)
        {
            var errors = await domainValidator.ValidateAsync(draftApprenticeshipDetails);
            errors.ThrowIfAny();
        }
    }
}
