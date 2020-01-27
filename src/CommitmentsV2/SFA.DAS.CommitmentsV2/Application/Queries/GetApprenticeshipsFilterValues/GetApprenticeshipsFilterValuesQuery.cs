using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQuery : IRequest<GetApprenticeshipsFilterValuesQueryResult>
    {
        public long ProviderId { get; set; }
    }
}
