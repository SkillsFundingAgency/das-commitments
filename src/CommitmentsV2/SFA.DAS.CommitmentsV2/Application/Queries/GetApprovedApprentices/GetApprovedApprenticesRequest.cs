using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesRequest : IRequest<GetApprovedApprenticesResponse>
    {
        public uint ProviderId { get; set; }
    }
}
