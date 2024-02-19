using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private IEnumerable<Error> ValidatePriceReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minPriceReduction)
        {
            if (!string.IsNullOrWhiteSpace(csvRecord.PriceReducedByAsString) && csvRecord.RecognisePriorLearning.GetValueOrDefault() == false)
            {
                yield return new Error("PriceReducedBy", "The <b>price this apprenticeship has been reduced by</b> due to prior learning should not be entered when recognise prior learning is false");
                yield break;
            }

            if (!string.IsNullOrWhiteSpace(csvRecord.PriceReducedByAsString))
            {
                if (csvRecord.PriceReducedBy == null)
                {
                    yield return new Error("PriceReducedBy", $"Total <b>price reduction</b> due to RPL must be a number between {minPriceReduction.ToString("N0")} and 18,000");
                }
                else if(csvRecord.PriceReducedBy > 18000)
                {
                    yield return new Error("PriceReducedBy", "Total <b>price reduction</b> due to RPL must be 18,000 or less");
                }
                else if (csvRecord.PriceReducedBy < minPriceReduction)
                {
                    yield return new Error("PriceReducedBy", $"Total <b>price reduction</b> due to RPL must be {minPriceReduction.ToString("N0")} pounds or more");
                };
            }
        }
    }
}