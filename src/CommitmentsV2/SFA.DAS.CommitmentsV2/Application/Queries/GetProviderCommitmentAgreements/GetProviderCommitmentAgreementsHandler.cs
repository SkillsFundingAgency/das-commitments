using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;

public class GetProviderCommitmentAgreementsHandler(
    Lazy<ProviderCommitmentsDbContext> db,
    ILogger<GetProviderCommitmentAgreementsHandler> logger,
    IProviderRelationshipsApiClient providerRelationshipsApiClient)
    : IRequestHandler<GetProviderCommitmentAgreementQuery, GetProviderCommitmentAgreementResult>
{
    public async Task<GetProviderCommitmentAgreementResult> Handle(GetProviderCommitmentAgreementQuery command, CancellationToken cancellationToken)
    {
        try
        {
            var cohortsAgreements = new List<ProviderCommitmentAgreement>();

            var agreements = await (from c in db.Value.Cohorts
                                    join a in db.Value.AccountLegalEntities on c.AccountLegalEntityId equals a.Id
                                    where c.ProviderId == command.ProviderId && !c.IsDeleted
                                    select new ProviderCommitmentAgreement
                                    {
                                        LegalEntityName = c.AccountLegalEntity.Name,
                                        AccountLegalEntityPublicHashedId = a.PublicHashedId
                                    }).ToListAsync(cancellationToken).ConfigureAwait(false);

            var permissionCheckRequest = new GetAccountProviderLegalEntitiesWithPermissionRequest
            {
                Operations = (int)Operation.CreateCohort,
                Ukprn = command.ProviderId
            };

            var permittedEmployers = await providerRelationshipsApiClient
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
            logger.LogError(exception, "Exception caught in GetProviderCommitmentAgreementsHandler.");
            throw;
        }
    }
}