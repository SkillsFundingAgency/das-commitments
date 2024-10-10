using SFA.DAS.CommitmentsV2.Infrastructure;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces;

public interface IApprovalsOuterApiClient
{
    Task<TResponse> Get<TResponse>(IGetApiRequest request);
    Task<TResponse> GetWithRetry<TResponse>(IGetApiRequest request);
    Task<ApiResponse<TResponse>> PostWithResponseCode<TData, TResponse>(IPostApiRequest<TData> request, bool includeResponse = true) where TData : class, new();
}