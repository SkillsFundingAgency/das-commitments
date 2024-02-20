using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private IEnumerable<Error> ValidatePriorLearning(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        if (csvRecord.StartDate < Constants.RecognisePriorLearningBecomesRequiredOn)
        {
            if (csvRecord.RecognisePriorLearning != null ||
                csvRecord.DurationReducedBy != null ||
                csvRecord.PriceReducedBy != null)
            {
                yield return new Error("RecognisePriorLearning", "<b>RPL data</b> should not be entered when the start date is before 1 August 2022.");
            }

            yield break;
        }

        if (csvRecord.RecognisePriorLearning == false)
        {
            if (csvRecord.DurationReducedBy != null)
            {
                yield return new Error("DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning should not be entered when recognise prior learning is false.");
            }

            if (csvRecord.PriceReducedBy != null)
            {
                yield return new Error("PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning should not be entered when recognise prior learning is false.");
            }
                
            yield break;
        }

        if (csvRecord.RecognisePriorLearning == null && csvRecord.RecognisePriorLearningAsString != null)
        {
            yield return new Error("RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised as 'true' or 'false'.");
        }

        if (csvRecord.RecognisePriorLearning == null && csvRecord.StartDate >= Constants.RecognisePriorLearningBecomesRequiredOn)
        {
            yield return new Error("RecognisePriorLearning", "Enter whether <b>prior learning</b> is recognised.");
        }

        if (csvRecord.DurationReducedBy == null)
        {
            yield return new Error("DurationReducedBy", "Enter the <b>duration</b> this apprenticeship has been reduced by due to prior learning in weeks using numbers only.");
        }
        else if (csvRecord.DurationReducedBy < 0)
        {
            yield return new Error("DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning must 0 or more.");
        }
        else if (csvRecord.DurationReducedBy > 999)
        {
            yield return new Error("DurationReducedBy", "The <b>duration</b> this apprenticeship has been reduced by due to prior learning must be 999 or less.");
        }

        if (csvRecord.PriceReducedBy == null)
        {
            yield return new Error("PriceReducedBy", "Enter the <b>price</b> this apprenticeship has been reduced by due to prior learning using numbers only.");
        }
        else if (csvRecord.PriceReducedBy < 0)
        {
            yield return new Error("PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning must be 0 or more.");
        }
        else if (csvRecord.PriceReducedBy > 100000)
        {
            yield return new Error("PriceReducedBy", "The <b>price</b> this apprenticeship has been reduced by due to prior learning must be £100,000 or less.");
        }

        // Check just in case they upload a new format file (and are not in the Pilot)
        if (csvRecord.TrainingTotalHoursAsString != null ||
            csvRecord.TrainingHoursReductionAsString != null ||
            csvRecord.IsDurationReducedByRPLAsString != null)
        {
            yield return new Error("RecognisePriorLearning", "<b>New RPL data</b> should not be entered as you not on the RPL pilot.");
        }
    }
}