using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        if (!string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString) && !csvRecord.RecognisePriorLearning.GetValueOrDefault())
        {
            yield return new Error("IsDurationReducedByRPL", "True or false should not be selected for <b>duration reduced</b> when recognise prior learning is false.");
            yield break;
        }

        if (IsRplRequired(csvRecord) && string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString))
        {
            yield return new Error("IsDurationReducedByRPL", "You must select true or false for <b>duration reduced</b>");
            yield break;
        }

        if (IsRplRequired(csvRecord) &&
            !string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString) &&
            csvRecord.IsDurationReducedByRPL == null)
        {
            yield return new Error("IsDurationReducedByRPL", "Enter whether <b>duration reduced</b> is 'true' or 'false'.");
            yield break;
        }

        if (csvRecord.IsDurationReducedByRPL == true)
        {
            if (string.IsNullOrEmpty(csvRecord.DurationReducedByAsString))
            {
                if (IsRplRequired(csvRecord))
                {
                    yield return new Error("DurationReducedBy", "You must enter the <b>duration this apprenticeship has been reduced by</b> due to prior learning");
                }

                yield break;
            }

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
        else if (csvRecord.IsDurationReducedByRPL == false && !string.IsNullOrEmpty(csvRecord.DurationReducedByAsString))
        {
            yield return new Error("DurationReducedBy", "The <b>duration this apprenticeship has been reduced by</b> due to prior learning should not be entered when reduction of duration by RPL is false.");
        }
    }
}
