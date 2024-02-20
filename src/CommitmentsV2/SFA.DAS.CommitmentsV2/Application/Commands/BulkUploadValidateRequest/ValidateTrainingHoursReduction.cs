using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateTrainingHoursReduction(BulkUploadAddDraftApprenticeshipRequest csvRecord, int maxTrainingHoursReduction)
    {
        if (!string.IsNullOrWhiteSpace(csvRecord.TrainingHoursReductionAsString) && csvRecord.RecognisePriorLearning.GetValueOrDefault() == false)
        {
            yield return new Error("TrainingTotalHours", "Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and 999");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(csvRecord.TrainingHoursReductionAsString))
        {
            yield break;
        }

        if (csvRecord.TrainingHoursReduction != null)
        {
            if (csvRecord.TrainingHoursReduction.Value > maxTrainingHoursReduction)
            {
                yield return new Error("TrainingHoursReduction", $"Total <b>reduction in off-the-job training time</b> due to RPL must be {maxTrainingHoursReduction.ToString("N0")} hours or less");
            }
            else if (csvRecord.TrainingHoursReduction.Value < 1)
            {
                yield return new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more");
            }
            else if (csvRecord.TrainingTotalHours != null)
            {
                if (csvRecord.TrainingTotalHours < csvRecord.TrainingHoursReduction)
                {
                    yield return new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be lower than the total off-the-job training time for this apprenticeship standard");
                }
                if (csvRecord.TrainingTotalHours - csvRecord.TrainingHoursReduction < 278)
                {
                    yield return new Error("TrainingHoursReduction", "The remaining off-the-job training is below the minimum 278 hours required for funding. Check if the <b>RPL reduction</b> is too high");
                }
            }
        }
        else
        {
            yield return new Error("TrainingHoursReduction", $"Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and {maxTrainingHoursReduction.ToString("N0")}");
        }
    }
}