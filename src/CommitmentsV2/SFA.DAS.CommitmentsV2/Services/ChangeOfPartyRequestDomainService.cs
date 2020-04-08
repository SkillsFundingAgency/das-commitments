using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;
using SFA.DAS.ProviderRelationships.Types.Models;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ChangeOfPartyRequestDomainService : IChangeOfPartyRequestDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IProviderRelationshipsApiClient _providerRelationshipsApiClient;

        public ChangeOfPartyRequestDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService, ICurrentDateTime currentDateTime, IProviderRelationshipsApiClient providerRelationshipsApiClient)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _currentDateTime = currentDateTime;
            _providerRelationshipsApiClient = providerRelationshipsApiClient;
        }

        public async Task<ChangeOfPartyRequest> CreateChangeOfPartyRequest(
            long apprenticeshipId,
            ChangeOfPartyRequestType changeOfPartyRequestType,
            long newPartyId,
            int price,
            DateTime startDate,
            DateTime? endDate,
            UserInfo userInfo,
            CancellationToken cancellationToken)
        {
            var party = _authenticationService.GetUserParty();

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, cancellationToken);

            //if (party == Party.Provider && changeOfPartyRequestType == ChangeOfPartyRequestType.ChangeEmployer)
            //{
            //    await CheckProviderHasPermission(apprenticeship.Cohort.ProviderId, newPartyId);
            //}

            var result = apprenticeship.CreateChangeOfPartyRequest(changeOfPartyRequestType,
                party,
                newPartyId,
                price,
                startDate,
                endDate,
                userInfo,
                _currentDateTime.UtcNow);

            _dbContext.Value.ChangeOfPartyRequests.Add(result);

            return result;
        }

        private async Task CheckProviderHasPermission(long providerId, long accountLegalEntityId)
        {
            var permissionsRequest = new GetAccountProviderLegalEntitiesWithPermissionRequest
            {
                Ukprn = providerId,
                Operation = Operation.CreateCohort
            };

            var permissions = await
                _providerRelationshipsApiClient.GetAccountProviderLegalEntitiesWithPermission(permissionsRequest);

            if (permissions.AccountProviderLegalEntities.All(x => x.AccountLegalEntityId != accountLegalEntityId))
            {
                throw new DomainException(nameof(accountLegalEntityId), $"Provider {providerId} does not have {nameof(Operation.CreateCohort)} permission for AccountLegalEntity {accountLegalEntityId} in order to create a Change of Party request");
            }
        }
    }
}
