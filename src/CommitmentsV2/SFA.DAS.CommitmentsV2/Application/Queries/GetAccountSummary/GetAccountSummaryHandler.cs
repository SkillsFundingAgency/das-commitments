using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Expressions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryHandler : IRequestHandler<GetAccountSummaryRequest, GetAccountSummaryResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetAccountSummaryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetAccountSummaryResponse> HandleOld(GetAccountSummaryRequest request,
            CancellationToken cancellationToken)
        {
            //todo: what if account doesn't exist? do we pay the cost of account.GetById in order to do a 404 if not?

            var result = await (_dbContext.Value
                .Cohorts
                .Where(c => c.EmployerAccountId == request.AccountId)
                .Select(c =>
                    new
                    {
                        IsApproved = c.EditStatus == EditStatus.Both &&
                                     (!c.TransferSenderId.HasValue ||
                                      c.TransferApprovalStatus == TransferApprovalStatus.Approved)
                    })).ToListAsync(cancellationToken);

            return new GetAccountSummaryResponse
            {
                AccountId = request.AccountId,
                HasCohorts = result.Any(x => !x.IsApproved),
                HasApprenticeships = result.Any(x => x.IsApproved)
            };

        }


        public async Task<GetAccountSummaryResponse> HandleTwoQueries(GetAccountSummaryRequest request,
            CancellationToken cancellationToken)
        {

            // This is probably more efficient but the query logic is duplicated

            var hasCohorts = await _dbContext.Value.Cohorts
                .AnyAsync(c => c.EmployerAccountId == request.AccountId &&
                               !(c.EditStatus == EditStatus.Both && (!c.TransferSenderId.HasValue ||
                                                                     c.TransferApprovalStatus ==
                                                                     TransferApprovalStatus.Approved)), cancellationToken: cancellationToken);
            var hasApprenticeships = await _dbContext.Value.Cohorts
                .AnyAsync(c => c.EmployerAccountId == request.AccountId &&
                               (c.EditStatus == EditStatus.Both && (!c.TransferSenderId.HasValue ||
                                                                     c.TransferApprovalStatus ==
                                                                     TransferApprovalStatus.Approved)), cancellationToken: cancellationToken);
            return new GetAccountSummaryResponse
            {
                AccountId = request.AccountId,
                HasCohorts = hasCohorts,
                HasApprenticeships = hasApprenticeships
            };
        }


        public async Task<GetAccountSummaryResponse> Handle(GetAccountSummaryRequest request,
            CancellationToken cancellationToken)
        {
            // The predicateBuilder allows expressions to be chained together, the IsApproved logic was getting duplicated in multiple queries 
            // this allows us to centralise it. It also will allow complex queries to be built up without putting lots of conditional logic into the where clause
            // or having to write multiple queries. code comes from 'c# 7 In a NutShell' the only additional I made was to add the AndNot() function

            var accountQuery = PredicateBuilder.True<Cohort>().And(c => c.EmployerAccountId == request.AccountId);

            var hasCohorts = await _dbContext.Value.Cohorts
                .AnyAsync(accountQuery.And(CohortQueries.IsApproved()), cancellationToken: cancellationToken);

            var hasApprenticeships = await _dbContext.Value.Cohorts
                .AnyAsync(accountQuery.AndNot(CohortQueries.IsApproved()), cancellationToken: cancellationToken);

            return new GetAccountSummaryResponse
            {
                AccountId = request.AccountId,
                HasCohorts = hasCohorts,
                HasApprenticeships = hasApprenticeships
            };

        }

        //public async Task<GetAccountSummaryResponse> HandleGB(GetAccountSummaryRequest request,
        //    CancellationToken cancellationToken)
        //{
        //    //todo: what if account doesn't exist? do we pay the cost of account.GetById in order to do a 404 if not?

        //    var result = _dbContext.Value
        //        .Cohorts
        //        .Where(c => c.EmployerAccountId == request.AccountId)
        //        .GroupBy(CohortQueries.IsApproved())
        //        ).Select((g) => new
        //        {
        //            g.Key,
        //            //g.Key.TransferSenderId,
        //            //g.Key.TransferApprovalStatus,
        //            Exists = g.Any()
        //        });

        //    //    ).Select((g) => new {
        //    //    IsApproved = g.Key.EditStatus == EditStatus.Both &&
        //    //                 (!g.Key.TransferSenderId.HasValue || g.Key.TransferApprovalStatus == TransferApprovalStatus.Approved),
        //    //    Count = g.Any()
        //    //});


        //    var summary = await result.ToListAsync(cancellationToken);

        //    return new GetAccountSummaryResponse
        //    {
        //        AccountId = request.AccountId,
        //        HasCohorts = result.Any(),
        //        HasApprenticeships = result.Any()
        //    };

        //}

    }
}
