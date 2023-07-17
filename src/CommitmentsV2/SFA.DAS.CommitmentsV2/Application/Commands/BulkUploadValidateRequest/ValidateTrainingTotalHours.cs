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
            if (!string.IsNullOrEmpty(csvRecord.TrainingTotalHoursAsString) && csvRecord.RecognisePriorLearning.GetValueOrDefault() == false)
            {
                yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> cannot be set when there is no prior learning");
                yield break;
            }
            
            if (!string.IsNullOrEmpty(csvRecord.TrainingTotalHoursAsString))
            {
                if (csvRecord.TrainingTotalHours == null)
                {
                    yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 278 and 9,999");
                }
                else if (csvRecord.TrainingTotalHours.Value > 9999)
                {
                    yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 9,999 hours or less");
                }
                else if (csvRecord.TrainingTotalHours.Value < 278)
                {
                    yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be 278 hours or more");
                }
                else
                {
                    yield return new Error("TrainingTotalHours", "Total <b>off-the-job training time</b> for this apprenticeship standard must be a number between 278 and 9,999");
                }
            }
        }
    }
}