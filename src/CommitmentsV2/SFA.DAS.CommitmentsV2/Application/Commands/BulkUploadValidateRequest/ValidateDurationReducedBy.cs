﻿using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            if (!string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString) && csvRecord.RecognisePriorLearning.GetValueOrDefault() == false)
            {
                yield return new Error("IsDurationReducedByRPL", "True or false should not be selected for duration reduced when recognise prior learning is false.");
                yield break;
            }

            if (!string.IsNullOrEmpty(csvRecord.DurationReducedByAsString) && !string.IsNullOrEmpty(csvRecord.IsDurationReducedByRPLAsString))
            {
                if (csvRecord.IsDurationReducedByRPL == true)
                {
                    if (csvRecord.DurationReducedBy != null)
                    {
                        if (csvRecord.DurationReducedBy.Value > 260)
                        {
                            yield return new Error("DurationReducedBy", "Reduction in duration must be 260 weeks or less.");
                        }
                        else if (csvRecord.DurationReducedBy.Value < 1)
                        {
                            yield return new Error("DurationReducedBy", "Reduction in duration must be 1 week or more.");
                        }
                        else  
                        {
                            yield return new Error("DurationReducedBy", "Reduction in duration must be a number between 1 and 260.");
                        }
                    }
                    else
                    {
                        yield return new Error("DurationReducedBy", "Reduction in duration must be a number between 1 and 260.");
                    }
                }
                else
                {
                    yield return new Error("DurationReducedBy", "The duration this apprenticeship has been reduced by due to prior learning should not be entered when reduction of duration by RPL is false.");
                }
            }
        }
    }
}