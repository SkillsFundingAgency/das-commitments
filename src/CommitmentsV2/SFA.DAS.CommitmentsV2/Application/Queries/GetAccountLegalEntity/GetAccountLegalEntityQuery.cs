namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity
{
    public class GetAccountLegalEntityQuery : IRequest<GetAccountLegalEntityQueryResult>
    {
        public long AccountLegalEntityId { get; set; }
    }
}
