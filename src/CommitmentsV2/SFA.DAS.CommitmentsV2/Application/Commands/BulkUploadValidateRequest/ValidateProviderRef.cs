﻿using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;


namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateProviderRef(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        var domainErrors = new List<Error>();
        if (!string.IsNullOrEmpty(csvRecord.ProviderRef) && csvRecord.ProviderRef.Length > 20)
        {
            domainErrors.Add(new Error("ProviderRef", "The <b>Provider Ref</b> must not be longer than 20 characters"));
        }

        return domainErrors;
    }
}