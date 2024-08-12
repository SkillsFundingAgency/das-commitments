using System.Net.Http;
using System.Threading;
using SFA.DAS.ProviderRelationships.Api.Client;

namespace SFA.DAS.CommitmentsV2.Shared.Services;

public sealed class StubProviderRelationshipsApiClient : IProviderRelationshipsApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUri = "http://localhost:3999/provider-relationships/api/";

    public StubProviderRelationshipsApiClient()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(BaseUri) };
    }

    private async Task<IList<AccountProviderLegalEntityDto>> GetPermissionsForProvider(long providerId, int operation, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"{providerId}", cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var items = JsonConvert.DeserializeObject<List<AccountProviderLegalEntityDtoWrapper>>(content);

        return items.Where(x => x.Permissions.Contains(~(Operation)operation)).Select(item => (AccountProviderLegalEntityDto)item).ToList();
    }

    public async Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(
        GetAccountProviderLegalEntitiesWithPermissionRequest withPermissionRequest,
        CancellationToken cancellationToken = new())
    {
        return new GetAccountProviderLegalEntitiesWithPermissionResponse
        {
            AccountProviderLegalEntities = await GetPermissionsForProvider(withPermissionRequest.Ukprn, (int)Operation.CreateCohort, cancellationToken).ConfigureAwait(false)
        };
    }

    public async Task<bool> HasPermission(HasPermissionRequest request, CancellationToken cancellationToken = new())
    {
        var result = await GetPermissionsForProvider(request.Ukprn, request.Operations, cancellationToken).ConfigureAwait(false);
        return result.Any();
    }

    public async Task<bool> HasRelationshipWithPermission(HasRelationshipWithPermissionRequest request,
        CancellationToken cancellationToken = new())
    {
        return (await GetAccountProviderLegalEntitiesWithPermission(
            new GetAccountProviderLegalEntitiesWithPermissionRequest
            {
                Ukprn = request.Ukprn,
                Operations = (int)request.Operation
            }, cancellationToken)).AccountProviderLegalEntities.Any();
    }

    public Task Ping(CancellationToken cancellationToken = new())
    {
        throw new NotImplementedException();
    }

    private sealed class AccountProviderLegalEntityDtoWrapper : AccountProviderLegalEntityDto
    {
        public List<Operation> Permissions { get; } = new();
    }
}