using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidateTrainingHoursReduction(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();

            if (string.IsNullOrEmpty(csvRecord.TrainingHoursReductionAsString))
            {
                //domainErrors.Add(new Error("TrainingTotalHours", ""));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingHoursReduction.Value > 999)
            {
                domainErrors.Add(new Error("TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be 999 hours or less"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingHoursReduction.Value < 1)
            {
                domainErrors.Add(new Error("TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be 1 hour or more"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingHoursReductionAsString.Contains(" "))
            {
                domainErrors.Add(new Error("TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && !Regex.IsMatch(csvRecord.TrainingHoursReductionAsString, "^([1-9]{1}([0-9]{1,2})?)+(,[0-9]{3})*$|^[1-9]{1}[0-9]*$"))
            {
                domainErrors.Add(new Error("TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && !Regex.IsMatch(csvRecord.TrainingHoursReductionAsString, "^[a-zA-Z0-9 ]*$"))
            {
                domainErrors.Add(new Error("TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be a number between 1 and 999"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingTotalHours != null && csvRecord.TrainingHoursReduction != null)
            {
                if (csvRecord.TrainingTotalHours.Value - csvRecord.TrainingHoursReduction.Value < 278)
                {
                    domainErrors.Add(new Error("TrainingHoursReduction", "The remaining off-the-job training is below the minimum 278 hours required for funding. Check if the RPL reduction is too high"));
                }
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingTotalHours != null && csvRecord.TrainingHoursReduction != null)
            {
                if (csvRecord.TrainingTotalHours.Value < csvRecord.TrainingHoursReduction.Value)
                {
                    domainErrors.Add(new Error("TrainingHoursReduction", "Total reduction in off-the-job training time due to RPL must be lower than the total off-the-job training time for this apprenticeship standard"));
                }
            }

            return domainErrors;
        }
    }
}