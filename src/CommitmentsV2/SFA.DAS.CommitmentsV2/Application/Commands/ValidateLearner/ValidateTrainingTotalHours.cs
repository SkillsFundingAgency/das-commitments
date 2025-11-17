using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<Error> ValidateTrainingTotalHours(BulkUploadAddDraftApprenticeshipRequest csvRecord, int minimumOffTheJobTrainingHoursForCourse)
    {
        if (!string.IsNullOrWhiteSpace(csvRecord.TrainingTotalHoursAsString) && !csvRecord.RecognisePriorLearning.GetValueOrDefault())
        {
            yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> cannot be set when there is no prior learning");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(csvRecord.TrainingTotalHoursAsString))
        {
            yield break;
        }
        
        if (csvRecord.TrainingTotalHours == null)
        {
            yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 187 and 9,999");
        }
        else if (csvRecord.TrainingTotalHours.Value > 9999)
        {
            yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 9,999 hours or less");
        }
        else
        {
            if (csvRecord.TrainingTotalHours.Value < minimumOffTheJobTrainingHoursForCourse)
            {
                yield return new Error("TrainingTotalHours", $"Total <b>off-the-job training time</b> for this apprenticeship standard must be {minimumOffTheJobTrainingHoursForCourse} hours or more");
            }
        }
    }
}