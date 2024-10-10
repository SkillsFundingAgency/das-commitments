namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;

public class GetProviderPaymentsPriorityQuery : IRequest<GetProviderPaymentsPriorityQueryResult>
{
    public long EmployerAccountId { get; }

    public GetProviderPaymentsPriorityQuery(long employerAccountId)
    {
        EmployerAccountId = employerAccountId;
    }
}