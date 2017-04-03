using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate
{
    [Validator(typeof(GetPendingApprenticeshipUpdateValidator))]
    public class GetPendingApprenticeshipUpdateRequest: IAsyncRequest<GetPendingApprenticeshipUpdateResponse>
    {
        public Caller Caller { get; set; }
        public long ApprenticeshipId { get; set; }
    }
}
