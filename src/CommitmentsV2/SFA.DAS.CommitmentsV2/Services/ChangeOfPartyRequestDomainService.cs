using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services;

public class ChangeOfPartyRequestDomainService(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IAuthenticationService authenticationService,
    ICurrentDateTime currentDateTime,
    IProviderRelationshipsApiClient providerRelationshipsApiClient,
    IOverlapCheckService overlapCheckService)
    : IChangeOfPartyRequestDomainService
{
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
        bool hasOverlappingTrainingDates,
        CancellationToken cancellationToken)
    {
        var party = authenticationService.GetUserParty();

        CheckPartyIsValid(party, changeOfPartyRequestType);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(apprenticeshipId, cancellationToken);

        if (changeOfPartyRequestType == ChangeOfPartyRequestType.ChangeProvider)
        {
            CheckEmployerHasNotSelectedTheirCurrentProvider(apprenticeship.Cohort.ProviderId, newPartyId,
                apprenticeship.Id);
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
            hasOverlappingTrainingDates,
            userInfo,
            currentDateTime.UtcNow);

        dbContext.Value.ChangeOfPartyRequests.Add(result);

        return result;
    }

    public async Task ValidateChangeOfEmployerOverlap(string uln, DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken)
    {
        var overlapResult =
            await overlapCheckService.CheckForOverlaps(uln, startDate.To(endDate), default, cancellationToken);

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

    private static void CheckPartyIsValid(Party party, ChangeOfPartyRequestType changeOfPartyRequestType)
    {
        if (party == Party.Provider && changeOfPartyRequestType != ChangeOfPartyRequestType.ChangeEmployer)
        {
            throw new DomainException(nameof(party),
                $"CreateChangeOfPartyRequest is restricted to Providers only - {party} is invalid");
        }

        if (party == Party.Employer && changeOfPartyRequestType != ChangeOfPartyRequestType.ChangeProvider)
        {
            throw new DomainException(nameof(party),
                $"CreateChangeOfPartyRequest is restricted to Employers only - {party} is invalid");
        }
    }

    private async Task CheckProviderHasPermission(long providerId, long accountLegalEntityId)
    {
        var permissionsRequest = new GetAccountProviderLegalEntitiesWithPermissionRequest
        {
            Ukprn = providerId,
            Operations = (int)Operation.CreateCohort
        };

        var permissions = await
            providerRelationshipsApiClient.GetAccountProviderLegalEntitiesWithPermission(permissionsRequest);

        if (permissions.AccountProviderLegalEntities.All(x => x.AccountLegalEntityId != accountLegalEntityId))
        {
            throw new DomainException(nameof(accountLegalEntityId),
                $"Provider {providerId} does not have {nameof(Operation.CreateCohort)} permission for AccountLegalEntity {accountLegalEntityId} in order to create a Change of Party request");
        }
    }

    private static void CheckEmployerHasNotSelectedTheirCurrentProvider(long currentProviderId, long newProviderId,
        long apprenticeshipId)
    {
        if (newProviderId == currentProviderId)
        {
            throw new DomainException("Ukprn",
                $"Provider {newProviderId} is already the training provider Apprenticeship {apprenticeshipId}");
        }
    }

    private static void CheckApprenticeIsNotAFlexiJob(DeliveryModel? dm, long apprenticeshipId)
    {
        if (dm == DeliveryModel.PortableFlexiJob)
        {
            throw new DomainException("DeliveryModel",
                $"Apprenticeship {apprenticeshipId} is a Portable Flexi-Job and cannot change training provider");
        }
    }
}