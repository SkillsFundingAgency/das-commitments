using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;

public class GetChangeOfPartyRequestsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetChangeOfPartyRequestsQuery, GetChangeOfPartyRequestsQueryResult>
{
    public async Task<GetChangeOfPartyRequestsQueryResult> Handle(GetChangeOfPartyRequestsQuery request, CancellationToken cancellationToken)
    {
        return new GetChangeOfPartyRequestsQueryResult
        {
            ChangeOfPartyRequests = await dbContext.Value
                .ChangeOfPartyRequests.Where(x => x.ApprenticeshipId == request.ApprenticeshipId)
                .Select(r => new GetChangeOfPartyRequestsQueryResult.ChangeOfPartyRequest
                {
                    Id = r.Id,
                    OriginatingParty = r.OriginatingParty,
                    ChangeOfPartyType = r.ChangeOfPartyType,
                    Status = r.Status,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,                        
                    Price = r.Price,
                    EmployerName = r.AccountLegalEntity.Name,
                    CohortId = r.Cohort.Id,
                    WithParty = r.Cohort.WithParty,
                    NewApprenticeshipId = r.NewApprenticeshipId,
                    ProviderId = r.ProviderId
                }).ToListAsync(cancellationToken)
        };
    }
}