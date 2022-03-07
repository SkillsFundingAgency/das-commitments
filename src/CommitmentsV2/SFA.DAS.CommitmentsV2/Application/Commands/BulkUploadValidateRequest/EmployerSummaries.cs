using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private class EmployerSummaries : List<EmployerSummary>
        {
            internal bool ContainsKey(string agreementId)
            {
                return this.Any(x => x.AgreementId == agreementId);
            }

            internal EmployerSummary GetValueOrDefault(string agreementId)
            {
                return this.First(x => x.AgreementId == agreementId);
            }
        }

        private class EmployerSummary
        {
            public EmployerSummary(string agreementId, long? legalEntityId, bool? isLevy, string name, bool? isSigned)
            {
                AgreementId = agreementId;
                LegalEntityId = legalEntityId;
                IsLevy = isLevy;
                Name = name;
                IsSigned = isSigned;
            }

            public string AgreementId { get; set; }
            public long? LegalEntityId { get; set; }
            public bool? IsLevy { get; set; }
            public string Name { get; set; }
            public bool? IsSigned { get; set; }
        }
    }
}
