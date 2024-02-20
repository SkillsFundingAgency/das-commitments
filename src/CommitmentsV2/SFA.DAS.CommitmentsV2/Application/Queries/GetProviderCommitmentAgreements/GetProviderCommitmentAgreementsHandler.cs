using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Types.Dtos;
using SFA.DAS.ProviderRelationships.Types.Models;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;

public class GetProviderCommitmentAgreementsHandler : IRequestHandler<GetProviderCommitmentAgreementQuery, GetProviderCommitmentAgreementResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly ILogger<GetProviderCommitmentAgreementsHandler> _logger;
    private readonly IProviderRelationshipsApiClient _providerRelationshipsApiClient;

    public GetProviderCommitmentAgreementsHandler(Lazy<ProviderCommitmentsDbContext> db,
        ILogger<GetProviderCommitmentAgreementsHandler> logger,
        IProviderRelationshipsApiClient providerRelationshipsApiClient)
    {
        _db = db;
        _logger = logger;
        _providerRelationshipsApiClient = providerRelationshipsApiClient;
    }

    public async Task<GetProviderCommitmentAgreementResult> Handle(GetProviderCommitmentAgreementQuery command, CancellationToken cancellationToken)
    {
        try
        {
            var cohortsAgreements = new List<ProviderCommitmentAgreement>();

            var agreements = await (from c in _db.Value.Cohorts
                join a in _db.Value.AccountLegalEntities on c.AccountLegalEntityId equals a.Id
                where c.ProviderId == command.ProviderId && !c.IsDeleted
                select new ProviderCommitmentAgreement
                {
                    LegalEntityName = c.AccountLegalEntity.Name,
                    AccountLegalEntityPublicHashedId = a.PublicHashedId
                }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var permissionCheckRequest = new GetAccountProviderLegalEntitiesWithPermissionRequest
            {
                Operation = Operation.CreateCohort,
                Ukprn = command.ProviderId
            };

            var permittedEmployers = await _providerRelationshipsApiClient
                .GetAccountProviderLegalEntitiesWithPermission(permissionCheckRequest, CancellationToken.None)
                .ConfigureAwait(false);

            var permittedCohortAgreements = permittedEmployers?
                .AccountProviderLegalEntities?.Select(dto => new ProviderCommitmentAgreement
                {
                    AccountLegalEntityPublicHashedId = dto.AccountLegalEntityPublicHashedId,
                    LegalEntityName = dto.AccountLegalEntityName
                }).ToList();

            if (agreements != null)
            {
                cohortsAgreements.AddRange(agreements);
            }

            if (permittedCohortAgreements != null)
            {
                cohortsAgreements.AddRange(permittedCohortAgreements);
            }

            var distinctAgreements = new List<ProviderCommitmentAgreement>();

            foreach (var cohortsAgreement in cohortsAgreements)
            {
                if (distinctAgreements.Exists(x => x.AccountLegalEntityPublicHashedId.Equals(cohortsAgreement.AccountLegalEntityPublicHashedId, StringComparison.InvariantCultureIgnoreCase)))
                    continue;

                distinctAgreements.Add(cohortsAgreement);
            }

            return new GetProviderCommitmentAgreementResult(distinctAgreements);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, exception.Message);
            throw;
        }
    }
}