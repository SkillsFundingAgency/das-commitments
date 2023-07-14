using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidateTrainingTotalHours(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();

            if (!string.IsNullOrEmpty(csvRecord.TrainingTotalHoursAsString) && csvRecord.RecognisePriorLearning.Value == true)
            {
                if (csvRecord.TrainingTotalHoursAsString.Contains(" "))
                {
                    domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be a number between 278 and 9,999"));
                }
                else if (csvRecord.TrainingTotalHoursAsString.All(char.IsDigit))
                {
                    if (csvRecord.TrainingTotalHours.Value > 9999)
                    {
                        domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be 9,999 hours or less"));
                    }
                    
                    if (csvRecord.TrainingTotalHours.Value < 278)
                    {
                        domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be 278 hours or more"));
                    }
                }
                else if (!csvRecord.TrainingTotalHoursAsString.All(char.IsDigit))
                {
                    bool isNumeric = int.TryParse(csvRecord.TrainingTotalHoursAsString, out int n);

                    if (isNumeric && n < 278)
                    {
                        domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be 278 hours or more"));
                    }
                    else
                    {
                        domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be a number between 278 and 9,999"));
                    }
                }
            }
            return domainErrors;
        }
    }
}