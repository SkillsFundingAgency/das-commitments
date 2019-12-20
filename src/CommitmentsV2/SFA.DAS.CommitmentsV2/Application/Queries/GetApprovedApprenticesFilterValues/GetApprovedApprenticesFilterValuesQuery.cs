using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQuery : IRequest<GetApprovedApprenticesFilterValuesResponse>
    {
        public uint ProviderId { get; set; }
    }
}
