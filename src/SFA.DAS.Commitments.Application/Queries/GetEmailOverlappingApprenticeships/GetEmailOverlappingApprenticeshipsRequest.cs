using System.Collections.Generic;
using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetEmailOverlappingApprenticeships
{
    [Validator(typeof(GetEmailOverlappingApprenticeshipsValidator))]
    public class GetEmailOverlappingApprenticeshipsRequest : IAsyncRequest<GetEmailOverlappingApprenticeshipsResponse>
    {
        public IList<ApprenticeshipEmailOverlapValidationRequest> OverlappingEmailApprenticeshipRequests { get; set; }
    }
}