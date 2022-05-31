using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;
using SFA.DAS.ProviderRelationships.Types.Models;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class StubProviderRelationshipsApiClient : IProviderRelationshipsApiClient
    {
        private readonly HttpClient _httpClient;

        public StubProviderRelationshipsApiClient()
        {
            _httpClient = new HttpClient { BaseAddress = new System.Uri("http://localhost:3999/provider-relationships-api/api/") };
        }

        private async Task<IList<AccountProviderLegalEntityDto>> GetPermissionsForProvider(long providerId, Operation operation, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"{providerId}", cancellationToken).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var items = JsonConvert.DeserializeObject<List<AccountProviderLegalEntityDtoWrapper>>(content);

            return items.Where(x => x.Permissions.Contains(operation)).Select(item => (AccountProviderLegalEntityDto)item).ToList();
        }

        public async Task<GetAccountProviderLegalEntitiesWithPermissionResponse> GetAccountProviderLegalEntitiesWithPermission(
            GetAccountProviderLegalEntitiesWithPermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return new GetAccountProviderLegalEntitiesWithPermissionResponse
            {
                AccountProviderLegalEntities = await GetPermissionsForProvider(request.Ukprn, request.Operation, cancellationToken).ConfigureAwait(false)
            };
        }

        public async Task<bool> HasPermission(HasPermissionRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = await GetPermissionsForProvider(request.Ukprn, request.Operation, cancellationToken).ConfigureAwait(false);
            return result.Any();
        }

        public async Task<bool> HasRelationshipWithPermission(HasRelationshipWithPermissionRequest request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return (await GetAccountProviderLegalEntitiesWithPermission(new GetAccountProviderLegalEntitiesWithPermissionRequest
            { Ukprn = request.Ukprn, Operation = request.Operation }, cancellationToken)).AccountProviderLegalEntities.Any();
        }

        public Task Ping(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task RevokePermissions(RevokePermissionsRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        public Task HealthCheck()
        {
            throw new System.NotImplementedException();
        }

        public class AccountProviderLegalEntityDtoWrapper : AccountProviderLegalEntityDto
        {
            public List<Operation> Permissions { get; set; }
        }

    }
}
