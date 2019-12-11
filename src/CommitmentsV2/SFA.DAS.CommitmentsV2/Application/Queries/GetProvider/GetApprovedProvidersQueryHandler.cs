﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider
{
    public class GetApprovedProvidersQueryHandler : IRequestHandler<GetApprovedProvidersQuery, GetApprovedProvidersQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetApprovedProvidersQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetApprovedProvidersQueryResult> Handle(GetApprovedProvidersQuery request, CancellationToken cancellationToken)
        {
            var accountQuery = PredicateBuilder.True<Cohort>().And(c => c.EmployerAccountId == request.AccountId);

            var result = await _db.Value.Cohorts.Where(accountQuery.And(CohortQueries.IsFullyApproved()))
                .Select(x => x.ProviderId.Value).ToListAsync(cancellationToken);

            return new GetApprovedProvidersQueryResult(result);
        }
    }
}
