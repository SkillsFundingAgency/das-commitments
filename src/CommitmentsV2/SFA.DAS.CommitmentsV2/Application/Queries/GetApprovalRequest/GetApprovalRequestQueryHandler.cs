using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using ApprovalFieldRequest = SFA.DAS.CommitmentsV2.Api.Types.Responses.ApprovalFieldRequest;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovalRequest;

public class GetApprovalRequestQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetApprovalRequestQuery, GetApprovalRequestQueryResult>
{
    public async Task<GetApprovalRequestQueryResult> Handle(GetApprovalRequestQuery query, CancellationToken cancellationToken)
    {
        var apprenticeship = await dbContext.Value.Apprenticeships
           .AsNoTracking()
           .Include(a => a.Cohort)
           .SingleOrDefaultAsync(a => a.Id == query.ApprenticeshipId, cancellationToken);

        if (apprenticeship == null)
        {
            return null;
        }

        if (apprenticeship.Cohort.EmployerAccountId != query.AccountId)
        {
            throw new UnauthorizedAccessException($"Employer {query.AccountId} cannot access apprenticeship {query.ApprenticeshipId}");
        }

        var fieldStatus = (CocApprovalItemStatus)query.CocApprovalItemStatus;
        var fieldRows = await dbContext.Value.CocApprovalItems
            .AsNoTracking()
            .Where(item => item.ApprenticeshipId == query.ApprenticeshipId
                && item.EmployerAcknowledgedAt == null
                && item.Status == fieldStatus)
            .OrderByDescending(item => item.Created)
            .ToListAsync(cancellationToken);

        var approvalRequests = fieldRows
            .Select(item => new ApprovalRequestItem
            {
                Id = item.Id,
                ApprenticeshipId = item.ApprenticeshipId,
                Status = (byte?)item.Status,
                Items = new List<ApprovalFieldRequest>
                {
                    new()
                    {
                        Field = item.Field,
                        Old = item.Old,
                        New = item.New,
                        Status = (byte?)item.Status,
                        Created = item.Created
                    }
                },
                Created = item.Created
            })
            .ToList();

        return new GetApprovalRequestQueryResult
        {
            ApprenticeName = $"{apprenticeship.FirstName} {apprenticeship.LastName}",
            ApprovalRequests = approvalRequests
        };
    }
}
