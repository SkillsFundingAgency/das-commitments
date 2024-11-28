using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity;

public class RemoveAccountLegalEntityCommandHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<RemoveAccountLegalEntityCommand>
{
    public async Task Handle(RemoveAccountLegalEntityCommand request, CancellationToken cancellationToken)
    {
        var account = await db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);
        
        var accountLegalEntity = await db.Value.AccountLegalEntities
            .IgnoreQueryFilters()
            .SingleAsync(ale => ale.Id == request.AccountLegalEntityId, cancellationToken);

        account.RemoveAccountLegalEntity(accountLegalEntity, request.Removed);

        if (accountLegalEntity.Deleted != null)
        {
            var cohorts = await db.Value.Cohorts.Include(c => c.Apprenticeships)
                .Where(c => c.AccountLegalEntityId == request.AccountLegalEntityId)
                .ToListAsync(cancellationToken);

            foreach (var cohort in cohorts)
            {
                if (cohort.Apprenticeships.Any(x => x.IsApproved))
                {
                    throw new DomainException(nameof(cohort), $"Cohort already has an approved apprenticeship");
                }

                cohort.Delete(cohort.WithParty, new UserInfo());
            }
        }
    }
}