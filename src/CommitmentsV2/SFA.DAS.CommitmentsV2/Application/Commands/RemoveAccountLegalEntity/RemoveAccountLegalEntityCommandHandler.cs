using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RemoveAccountLegalEntity
{
    public class RemoveAccountLegalEntityCommandHandler : AsyncRequestHandler<RemoveAccountLegalEntityCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        public RemoveAccountLegalEntityCommandHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        protected override async Task Handle(RemoveAccountLegalEntityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var account = await _db.Value.Accounts.SingleAsync(a => a.Id == request.AccountId, cancellationToken);
                var accountLegalEntity = await _db.Value.AccountLegalEntities
                    .IgnoreQueryFilters()
                    .SingleAsync(ale => ale.Id == request.AccountLegalEntityId, cancellationToken);

                account.RemoveAccountLegalEntity(accountLegalEntity, request.Removed);


                if (accountLegalEntity.Deleted != null)
                {
                    var cohorts = await _db.Value.Cohorts.Include(c => c.Apprenticeships)
                        .Where(c => c.AccountLegalEntityId == request.AccountLegalEntityId)
                        .ToListAsync();

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
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}