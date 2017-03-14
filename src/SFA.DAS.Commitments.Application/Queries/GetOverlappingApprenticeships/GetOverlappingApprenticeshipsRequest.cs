using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    [Validator(typeof(GetOverlappingApprenticeshipsValidator))]
    public class GetOverlappingApprenticeshipsRequest: IAsyncRequest<GetOverlappingApprenticeshipsResponse>
    {
        public IEnumerable<ApprenticeshipOverlapValidationRequest> OverlappingApprenticeshipRequests { get; set; }
    }
}
