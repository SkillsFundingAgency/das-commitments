using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        if (!string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString) && !csvRecord.RecognisePriorLearning.GetValueOrDefault())
        {
            yield return new Error("IsDurationReducedByRPL", "True or false should not be selected for <b>duration reduced</b> when recognise prior learning is false.");
            yield break;
        }

        if (string.IsNullOrEmpty(csvRecord.DurationReducedByAsString) ||
            string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString))
        {
            yield break;
        }

        if (csvRecord.IsDurationReducedByRPL == true)
        {
            if (csvRecord.DurationReducedBy != null)
            {
                switch (csvRecord.DurationReducedBy.Value)
                {
                    case > 260:
                        yield return new Error("DurationReducedBy", "<b>Reduction in duration</b> must be 260 weeks or less.");
                        break;
                    case < 1:
                        yield return new Error("DurationReducedBy", "<b>Reduction in duration</b> must be 1 week or more.");
                        break;
                }
            }
            else
            {
                yield return new Error("DurationReducedBy", "<b>Reduction in duration</b> must be a number between 1 and 260.");
            }
        }
        else
        {
            yield return new Error("DurationReducedBy", "The <b>duration this apprenticeship has been reduced by</b> due to prior learning should not be entered when reduction of duration by RPL is false.");
        }
    }
}