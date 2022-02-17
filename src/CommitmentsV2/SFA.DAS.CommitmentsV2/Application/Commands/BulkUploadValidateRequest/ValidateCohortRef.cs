using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;


namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateCohortRef(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.CohortRef))
            {
                domainErrors.Add(new Error("CohortRef", "<b>Cohort Ref</b> must be entered"));
            }
            else
            {
                var cohort = GetCohortDetails(csvRecord.CohortRef);

                if (cohort == null)
                {
                    domainErrors.Add(new Error("CohortRef", $"You must enter a valid <br>Cohort Ref</b>"));
                    return domainErrors;
                }
                else if (csvRecord.CohortRef.Length > 20)
                {
                    domainErrors.Add(new Error("CohortRef", $"You must enter a valid <br>Cohort Ref</b>"));
                }
                else if (cohort.AccountLegalEntity.PublicHashedId != csvRecord.AgreementId && !string.IsNullOrWhiteSpace(GetEmployerName(csvRecord.AgreementId)))
                {
                    domainErrors.Add(new Error("CohortRef", $"You must enter a valid <b>Cohort Ref</b>"));
                }

                if (cohort.WithParty == Types.Party.Employer)
                {
                    domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort, as it is with the employer. You need to <b>add this learner to a different or new cohort.</b>"));
                }
                if (cohort.WithParty == Types.Party.TransferSender)
                {
                    domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort, as it is with the transfer sending employer. You need to <b>add this learner to a different or new cohort.</b>"));
                }
                if (cohort.IsLinkedToChangeOfPartyRequest)
                {
                    domainErrors.Add(new Error("CohortRef", $"You cannot add apprentices to this cohort. You need to <b>add this learner to a different or new cohort.</b>"));
                }
                if (cohort.Apprenticeships.Count > 0)
                {
                    domainErrors.Add(new Error("CohortRef", $"This cohort is not empty. You need to <b>add this learner to a different or new cohort.</b>"));
                }
            }

            return domainErrors;
        }
    }
}
