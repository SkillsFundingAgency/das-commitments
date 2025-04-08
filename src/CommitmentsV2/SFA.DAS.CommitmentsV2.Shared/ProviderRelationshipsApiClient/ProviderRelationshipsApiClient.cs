using System.Net.Http;
using System.Threading;
using Microsoft.AspNetCore.WebUtilities;

namespace SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

public class ProviderRelationshipsApiClient : IProviderRelationshipsApiClient
{
    private readonly HttpClient _httpClient;

    public ProviderRelationshipsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(GetAccountProviderLegalEntitiesWithPermissionRequest withPermissionRequest, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(AddQueryString("accountproviderlegalentities", withPermissionRequest), UriKind.Relative);
        var response = await _httpClient.GetAsync(uri, cancellationToken);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Error when trying to invoke provider relationships api endpoint: accountproviderlegalentities");

        var providerPermissions = await response.Content.ReadAsAsync<GetAccountProviderLegalEntitiesWithPermissionResponse>();
        return providerPermissions;
    }

    public async Task<bool> HasPermission(HasPermissionRequest request, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(AddQueryString("permissions/has", request), UriKind.Relative);
        var response = await _httpClient.GetAsync(uri, cancellationToken);

        if (!response.IsSuccessStatusCode) throw new InvalidOperationException("Error when trying to invoke provider relationships api endpoint: permissions/has");

        return await response.Content.ReadAsAsync<bool>();
    }

    private static string AddQueryString(string uri, object queryData)
    {
        var queryDataDictionary = queryData.GetType().GetProperties().ToDictionary(x => x.Name, x => x.GetValue(queryData)?.ToString() ?? string.Empty);
        return QueryHelpers.AddQueryString(uri, queryDataDictionary);
    }
}