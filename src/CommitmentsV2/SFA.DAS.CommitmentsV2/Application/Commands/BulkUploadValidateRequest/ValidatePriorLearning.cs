using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidatePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            if(csvRecord.StartDate < Constants.RecognisePriorLearningBecomesRequiredOn)
            {
                yield break;
            }

            if (csvRecord.RecognisePriorLearning == false)
            {
                yield break;
            }

            if (csvRecord.RecognisePriorLearning == null)
            {
                //This validation cannot be enabled until the bulk upload file format change has been communicated
                //and software integrators have had time to update their systems.
                //yield return new Error("RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");

                // When the above validation is enabled, this one must be kept.
                // We don't want to return *ReducedBy errors until RPL is confirmed
                yield break;
            }

            if (csvRecord.DurationReducedBy == null)
            {
                yield return new Error("DurationReducedBy", "Enter the <b>duration</b> this apprenticeship has been reduced by due to prior learning.");
            } 
            else if (csvRecord.DurationReducedBy < 1)
            {
                yield return new Error("DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning must be more than 0.");
            }

            if (csvRecord.PriceReducedBy == null)
            {
                yield return new Error("PriceReducedBy", "Enter the <b>price</b> this apprenticeship has been reduced by due to prior learning.");
            }
            else if (csvRecord.PriceReducedBy < 1)
            {
                yield return new Error("PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning must be more than 0.");
            }
            else if (csvRecord.PriceReducedBy < 100000)
            {
                yield return new Error("PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning must be £100,000 or less.");
            }
        }
    }
}