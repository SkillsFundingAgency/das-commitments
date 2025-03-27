using System.Threading;

namespace SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

public interface IProviderRelationshipsApiClient
{
    Task<bool> HasPermission(HasPermissionRequest request, CancellationToken cancellationToken = default);
    Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(GetAccountProviderLegalEntitiesWithPermissionRequest withPermissionRequest, CancellationToken cancellationToken = default);
}