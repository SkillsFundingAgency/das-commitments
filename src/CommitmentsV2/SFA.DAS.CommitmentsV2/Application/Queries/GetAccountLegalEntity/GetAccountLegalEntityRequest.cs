using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity
{
    public class GetAccountLegalEntityRequest : IRequest<GetAccountLegalEntityResponse>
    {
        public long AccountLegalEntityId { get; set; }
    }
}
