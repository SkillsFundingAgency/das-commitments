using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateRecognisePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        if (csvRecord.StartDate < Constants.RecognisePriorLearningBecomesRequiredOn)
        {
            if (csvRecord.RecognisePriorLearning != null)
            {
                yield return new Error("RecognisePriorLearning", "<b>RPL data</b> should not be entered when the start date is before 1 August 2022.");
            }

            yield break;
        }

        if (csvRecord.RecognisePriorLearning == null && csvRecord.RecognisePriorLearningAsString != null)
        {
            yield return new Error("RecognisePriorLearning",
                "Enter whether <b>prior learning</b> is recognised as 'true' or 'false'.");
        }

        if (csvRecord.RecognisePriorLearning == null &&
            csvRecord.StartDate >= Constants.RecognisePriorLearningBecomesRequiredOn)
        {
            yield return new Error("RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
        }
    }
}