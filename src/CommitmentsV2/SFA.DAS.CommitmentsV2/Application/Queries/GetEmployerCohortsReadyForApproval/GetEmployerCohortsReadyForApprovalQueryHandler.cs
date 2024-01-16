using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval
{
    public class GetEmployerCohortsReadyForApprovalQueryHandler : IRequestHandler<GetEmployerCohortsReadyForApprovalQuery, GetEmployerCohortsReadyForApprovalQueryResults>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        public GetEmployerCohortsReadyForApprovalQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetEmployerCohortsReadyForApprovalQueryResults> Handle(GetEmployerCohortsReadyForApprovalQuery request, CancellationToken cancellationToken)
        {
            var db = _db.Value;

            var result = await db.Cohorts
          .Where(c => c.EmployerAccountId == request.EmployerAccountId &&
                       !c.IsDeleted &&
                       (c.Approvals == Party.Provider && c.WithParty == Party.Employer ||
                        c.Approvals == Party.None && c.WithParty == Party.Employer && c.IsDraft && c.Originator == Party.Employer.ToOriginator()))
              .Select(c => new GetEmployerCohortsReadyForApprovalQueryResult
              {
                  CohortId = c.Id,
                  AccountId = c.EmployerAccountId,
                  CohortReference = c.Reference,
                  AccountLegalEntityId = c.AccountLegalEntity.Id,
                  AccountLegalEntityPublicHashedId = c.AccountLegalEntity.PublicHashedId,
                  LegalEntityName = c.AccountLegalEntity.Name,
                  ProviderName = c.Provider.Name,
                  TransferSenderId = c.TransferSenderId,
                  TransferSenderName = c.TransferSender.Name,
                  ProviderId = c.ProviderId
              })
              .ToListAsync();

            return new GetEmployerCohortsReadyForApprovalQueryResults()
            {
                GetEmployerCohortsReadyForApprovalQueryResult = result
            };
        }
    }
}