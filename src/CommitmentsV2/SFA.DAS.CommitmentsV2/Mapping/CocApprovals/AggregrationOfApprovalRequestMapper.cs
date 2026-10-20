using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Models;


namespace SFA.DAS.CommitmentsV2.Mapping.CocApprovals;

public class AggregrationOfApprovalRequestMapper(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IModelMapper modelMapper,
    ILogger<CocApprovalRequestToCocApprovalDetailsMapper> logger) : IMapper<CocApprovalRequest, CocApprovalCommand>
{
    public async Task<CocApprovalCommand> Map(CocApprovalRequest request)
    {
        var previousPendingItems = await GetPreviousPendingItemsForLearningKey(request.LearningKey);
        if (!previousPendingItems.Any())
        {
            logger.LogInformation("Creating a brand new request for LearningKey {0}", request.LearningKey);
            return new CocApprovalCommand
            {
                CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
                Action = AggregrationAction.CreateNew,
            };
        }

        // Should we validate that the ApprenticeshipId and other fields match between the previous request and the new request?
        // There is nothing in the requirements that says we should, but it might be a good idea to ensure that the new request
        // is for the same apprenticeship and learning type as the previous request.

        var aggregatedItems = CreateAggregratedItems(request, previousPendingItems);

        if (aggregatedItems.Any())
        {
            logger.LogInformation("Creating a superseded request for LearningKey {0}", request.LearningKey);

            request.Changes = aggregatedItems;
            return new CocApprovalCommand
            {
                CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
                Action = AggregrationAction.SupersedePrevious,
            };
        }

        logger.LogInformation("Cancelling the pending requests for LearningKey {0}", request.LearningKey);
        return new CocApprovalCommand
        {
            CocApprovalDetails = null,
            Action = AggregrationAction.CancelPrevious,
        };
    }

    public async Task<List<CocApprovalItem>> GetPreviousPendingItemsForLearningKey(Guid learningKey)
    {
        var db = dbContext.Value;
        logger.LogInformation("Looking for previous pending requests for LearningKey {0}", learningKey);
        var pendingItems = await db.CocApprovalItems.Where(r => r.LearningKey == learningKey && r.Status == CocApprovalItemStatus.Pending).ToListAsync();
        return pendingItems;
    }

    public List<CocApprovalFieldChange> CreateAggregratedItems(CocApprovalRequest request, List<CocApprovalItem> previousItems)
    {
        var list = new List<CocApprovalFieldChange>();

        // Aggregate the changes from the new request with the previous request, 
        // keep new fields as they are, and for existing fields,
        // get the old value from the previous request and the new value from the new request.
        foreach (var change in request.Changes)
        {
            var previousChange = previousItems.FirstOrDefault(i => i.Field == change.ChangeType);
            if (previousChange == null)
            {
                list.Add(change);
            }
            else
            {
                var newChange = new CocApprovalFieldChange
                {
                    ChangeType = change.ChangeType,
                    Data = new CocData
                    {
                        Old = previousChange.Old,
                        New = change.Data.New
                    }
                };

                if(newChange.Data.Old == newChange.Data.New)
                {
                    logger.LogInformation("Change for field {Field} has no effective change, skipping", newChange.ChangeType);
                    continue;
                }
                list.Add(newChange);
            }
        }

        // Add any previous changes that are not in the new request, as they are still effective. I'm guessing here as the requirements don't specify what to do in this case,
        // but it seems reasonable to keep them.
        foreach (var item in previousItems)
        {
            var newChange = request.Changes.FirstOrDefault(c => c.ChangeType == item.Field);
            if (newChange == null)
            {
                var change = new CocApprovalFieldChange
                {
                    ChangeType = item.Field,
                    Data = new CocData
                    {
                        Old = item.Old,
                        New = item.New
                    }
                };
                list.Add(change);
            }
        }

        return list;
    }
}