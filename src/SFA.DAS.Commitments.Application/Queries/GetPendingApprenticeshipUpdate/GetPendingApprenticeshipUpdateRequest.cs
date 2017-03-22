using FluentValidation.Attributes;
using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetPendingApprenticeshipUpdate
{
    [Validator(typeof(GetPendingApprenticeshipUpdateValidator))]
    public class GetPendingApprenticeshipUpdateRequest: IAsyncRequest<GetPendingApprenticeshipUpdateResponse>
    {
        public long ApprenticeshipId { get; set; }
    }
}
