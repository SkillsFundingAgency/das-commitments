using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidatePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            // This validation cannot be enabled until the bulk upload file format change has been communicated
            // and software integrators have had time to update their systems.

            //if (csvRecord.RecognisePriorLearning == false)
            //{
            yield break;
            //}

            //if (csvRecord.RecognisePriorLearning == null)
            //{
            //    yield return new Error("RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
            //    yield break;
            //}

            //if (csvRecord.DurationReducedBy == null)
            //{
            //    yield return new Error("DurationReducedBy", "Enter the <b>duration</b> this apprenticeship has been reduced by due to prior learning.");
            //}

            //if (csvRecord.PriceReducedBy == null)
            //{
            //    yield return new Error("PriceReducedBy", "Enter the <b>price</b> this apprenticeship has been reduced by due to prior learning.");
            //}
        }
    }
}