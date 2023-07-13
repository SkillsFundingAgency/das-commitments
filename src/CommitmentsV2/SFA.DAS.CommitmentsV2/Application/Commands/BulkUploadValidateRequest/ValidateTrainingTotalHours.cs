using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidateTrainingTotalHours(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();

            if (string.IsNullOrEmpty(csvRecord.TrainingTotalHoursAsString))
            {
                //domainErrors.Add(new Error("TrainingTotalHours", ""));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingTotalHours.Value > 9999)
            {
                domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be 9,999 hours or less"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingTotalHours.Value < 278)
            {
                domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be 278 hours or more"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && csvRecord.TrainingTotalHoursAsString.Contains(" "))
            {
                domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be a number between 278 and 9,999"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && !Regex.IsMatch(csvRecord.TrainingTotalHoursAsString, "^([1-9]{1}([0-9]{1,2})?)+(,[0-9]{3})*$|^[1-9]{1}[0-9]*$"))
            {
                domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be a number between 278 and 9,999"));
            }
            else if (csvRecord.RecognisePriorLearning.Value == true && !Regex.IsMatch(csvRecord.TrainingTotalHoursAsString, "^[a-zA-Z0-9 ]*$"))
            {
                domainErrors.Add(new Error("TrainingTotalHours", "Total off-the-job training time for this apprenticeship standard must be a number between 278 and 9,999"));
            }

            return domainErrors;
        }
    }
}

