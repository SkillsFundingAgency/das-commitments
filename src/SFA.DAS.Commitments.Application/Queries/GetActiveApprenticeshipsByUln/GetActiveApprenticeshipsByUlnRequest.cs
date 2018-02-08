using FluentValidation.Attributes;
using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln
{
    [Validator(typeof(GetActiveApprenticeshipsByUlnValidator))]
    public class GetActiveApprenticeshipsByUlnRequest: IAsyncRequest<GetActiveApprenticeshipsByUlnResponse>
    {
        public string Uln { get; set; }
    }
}
