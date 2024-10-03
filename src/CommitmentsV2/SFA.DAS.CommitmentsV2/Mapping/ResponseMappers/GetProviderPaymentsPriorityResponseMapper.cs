using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderPaymentsPriority;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

public class GetProviderPaymentsPriorityResponseMapper : IMapper<GetProviderPaymentsPriorityQueryResult, GetProviderPaymentsPriorityResponse>
{
    public Task<GetProviderPaymentsPriorityResponse> Map(GetProviderPaymentsPriorityQueryResult source)
    {
        return Task.FromResult(new GetProviderPaymentsPriorityResponse
        {
            ProviderPaymentPriorities = source.PriorityItems.Select(r =>
                new GetProviderPaymentsPriorityResponse.ProviderPaymentPriorityItem
                {
                    ProviderName = r.ProviderName,
                    ProviderId = r.ProviderId,
                    PriorityOrder = r.PriorityOrder
                }).ToList()
        });
    }
}