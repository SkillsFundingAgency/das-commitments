using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidateTrainingHoursReduction(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();

            if (!string.IsNullOrEmpty(csvRecord.TrainingHoursReductionAsString) && csvRecord.RecognisePriorLearning.Value == true)
            {
                if (csvRecord.TrainingHoursReductionAsString.Contains(" "))
                {
                    domainErrors.Add(new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and 999"));
                }
                else if (csvRecord.TrainingHoursReductionAsString.All(char.IsDigit))
                {
                    if (csvRecord.TrainingHoursReduction.Value > 999)
                    {
                        domainErrors.Add(new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 999 hours or less"));
                    }

                    if (csvRecord.TrainingHoursReduction.Value < 1)
                    {
                        domainErrors.Add(new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more"));
                    }

                    bool isTrainingTotalHoursNumeric = int.TryParse(csvRecord.TrainingTotalHoursAsString, out int h);
                    if (isTrainingTotalHoursNumeric)
                    {
                        if (csvRecord.TrainingTotalHours < csvRecord.TrainingHoursReduction)
                        {
                            domainErrors.Add(new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be lower than the total off-the-job training time for this apprenticeship standard"));
                        }

                        if (csvRecord.TrainingTotalHours - csvRecord.TrainingHoursReduction < 278)
                        {
                            domainErrors.Add(new Error("TrainingHoursReduction", "The remaining off-the-job training is below the minimum 278 hours required for funding. Check if the <b>RPL reduction</b> is too high"));
                        }
                    }
                }
                else if (!csvRecord.TrainingHoursReductionAsString.All(char.IsDigit))
                {
                    bool isTrainingHoursReductionNumeric = int.TryParse(csvRecord.TrainingHoursReductionAsString, out int r);
                    if (isTrainingHoursReductionNumeric && r < 1)
                    {
                        domainErrors.Add(new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be 1 hour or more"));
                    }
                    else
                    {
                        domainErrors.Add(new Error("TrainingHoursReduction", "Total <b>reduction in off-the-job training time</b> due to RPL must be a number between 1 and 999"));
                    }
                }
            }
            return domainErrors;
        }
    }
}