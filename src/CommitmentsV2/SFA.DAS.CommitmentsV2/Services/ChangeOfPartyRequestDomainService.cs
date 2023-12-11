using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
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
        private readonly IOverlapCheckService _overlapCheckService;

        public ChangeOfPartyRequestDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService,
            ICurrentDateTime currentDateTime, 
            IProviderRelationshipsApiClient providerRelationshipsApiClient
            , IOverlapCheckService overlapCheckService)
        {
            _dbContext = dbContext;
            _authenticationService = authenticationService;
            _currentDateTime = currentDateTime;
            _providerRelationshipsApiClient = providerRelationshipsApiClient;
            _overlapCheckService = overlapCheckService;
        }

        public async Task<ChangeOfPartyRequest> CreateChangeOfPartyRequest(
            long apprenticeshipId,
            ChangeOfPartyRequestType changeOfPartyRequestType,
            long newPartyId,
            int? price,
            DateTime? startDate,
            DateTime? endDate,
            UserInfo userInfo,
            int? employmentPrice,
            DateTime? employmentEndDate,
            DeliveryModel? deliveryModel,
            bool hasOltd,
            CancellationToken cancellationToken)
        {
              var party = _authenticationService.GetUserParty();

            CheckPartyIsValid(party, changeOfPartyRequestType);

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, cancellationToken);
            
            if (changeOfPartyRequestType == ChangeOfPartyRequestType.ChangeProvider)
            {
                CheckEmployerHasntSelectedTheirCurrentProvider(apprenticeship.Cohort.ProviderId, newPartyId, apprenticeship.Id);
                CheckApprenticeIsNotAFlexiJob(apprenticeship.DeliveryModel, apprenticeshipId);
            }
            
            if (party == Party.Provider && changeOfPartyRequestType == ChangeOfPartyRequestType.ChangeEmployer)
            {
                await CheckProviderHasPermission(apprenticeship.Cohort.ProviderId, newPartyId);
            }

            var result = apprenticeship.CreateChangeOfPartyRequest(changeOfPartyRequestType,
            party,
            newPartyId,
            price,
            startDate,
            endDate,
            employmentPrice,
            employmentEndDate,
            deliveryModel,
            hasOltd,
            userInfo,
            _currentDateTime.UtcNow);

            _dbContext.Value.ChangeOfPartyRequests.Add(result);

            return result;
        }

        public async Task ValidateChangeOfEmployerOverlap(string uln, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {            
            var overlapResult = await _overlapCheckService.CheckForOverlaps(uln, startDate.To(endDate), default, cancellationToken);

            if (!overlapResult.HasOverlaps) return;

            var errorMessage = "The date overlaps with existing dates for the same apprentice."
                               + Environment.NewLine +
                               "Please check the date - contact the employer for help";

            var errors = new List<DomainError>();

            // allow HasOverlappingStartDate on its own
            if (overlapResult.HasOverlappingEndDate && overlapResult.HasOverlappingStartDate)
            {
                errors.Add(new DomainError(nameof(startDate), errorMessage));
            }

            if (overlapResult.HasOverlappingEndDate)
            {
                errors.Add(new DomainError(nameof(endDate), errorMessage));
            }

            if (errors.Count > 0)
            {
                throw new DomainException(errors);
            }
        }

        private void CheckPartyIsValid(Party party, ChangeOfPartyRequestType changeOfPartyRequestType)
        {
            if (party == Party.Provider && changeOfPartyRequestType != ChangeOfPartyRequestType.ChangeEmployer)
            {
                throw new DomainException(nameof(party), $"CreateChangeOfPartyRequest is restricted to Providers only - {party} is invalid");
            }

            if (party == Party.Employer && changeOfPartyRequestType != ChangeOfPartyRequestType.ChangeProvider)
            {
                throw new DomainException(nameof(party), $"CreateChangeOfPartyRequest is restricted to Employers only - {party} is invalid");
            }
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

        private void CheckEmployerHasntSelectedTheirCurrentProvider(long currentProviderId, long newProviderId, long apprenticeshipId)
        {
            if (newProviderId == currentProviderId)
            {
                throw new DomainException("Ukprn", $"Provider {newProviderId} is already the training provider Apprenticeship {apprenticeshipId}");
            }
        }

        private void CheckApprenticeIsNotAFlexiJob(DeliveryModel? dm, long apprenticeshipId)
        {
            if (dm == DeliveryModel.PortableFlexiJob)
            {
                throw new DomainException("DeliveryModel", $"Apprenticeship {apprenticeshipId} is a Portable Flexi-Job and cannot change training provider");
            }
        }
    }
}
