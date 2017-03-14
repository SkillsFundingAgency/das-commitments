using System;
using System.Collections.Generic;
using FluentValidation.Attributes;
using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships
{
    [Validator(typeof(GetOverlappingApprenticeshipsValidator))]
    public class GetOverlappingApprenticeshipsRequest: IAsyncRequest<GetOverlappingApprenticeshipsResponse>
    {
        public IEnumerable<OverlappingApprenticeshipRequest> OverlappingApprenticeshipRequests { get; set; }
    }

    //todo: replace this class with an api type
    public class OverlappingApprenticeshipRequest
    {
        public long? ExcludeApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
