using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;

public class GetApprovalRequestQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprovalRequestQuery, GetApprovalRequestQueryResult>
{
    public async Task<GetApprovalRequestQueryResult> Handle(GetApprovalRequestQuery query, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.Apprenticeships
           .AsNoTracking()
           .Include(a => a.Cohort)
           .Include(a => a.ApprovalRequests)
           .ThenInclude(request => request.Items)
           .SingleOrDefaultAsync(a => a.Id == query.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            return null;
        }

        if (apprenticeship.Cohort.EmployerAccountId != query.AccountId)
        {
            throw new UnauthorizedAccessException($"Employer {query.AccountId} cannot access apprenticeship {query.ApprenticeshipId}");
        }

        var approvalRequests = (apprenticeship.ApprovalRequests ?? [])
            .Where(x => x.ApprenticeshipId == query.ApprenticeshipId
            && x.EmployerAcknowledgedBy == null
            && x.EmployerAcknowledgedAt == null
            && x.Items.Any(i => i.Status == (Models.CocApprovalItemStatus)query.CocApprovalItemStatus))
            .Select(x => new ApprovalRequestItem
            {
                Id = x.Id,
                ApprenticeshipId = x.ApprenticeshipId,
                Status = (byte?)x.Status,
                Items = x.Items.Where(i => i.Status == (Models.CocApprovalItemStatus)query.CocApprovalItemStatus)
                .Select(i => new ApprovalFieldRequest
                {
                    Field = i.Field,
                    Old = i.Old,
                    New = i.New,
                    Status = (byte?)i.Status,
                    Created = i.Created
                }).ToList(),
                Created = x.Created
            })
            .OrderByDescending(x => x.Created).
            ToList();

        return new GetApprovalRequestQueryResult
        {
            ApprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
            ApprovalRequests = approvalRequests
        };
    }
}