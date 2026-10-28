using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Data;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Mapping.CocApprovals;

public class AggregationOfApprovalRequestMapper(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IModelMapper modelMapper,
    ILogger<CocApprovalRequestToCocApprovalDetailsMapper> logger) : IMapper<CocApprovalRequest, CocApprovalCommand>
{
    public async Task<CocApprovalCommand> Map(CocApprovalRequest request)
    {
        var previousPendingItems = await GetPreviousPendingItemsForLearningKey(request.LearningKey);
        var nonPricePendingItems = previousPendingItems
            .Where(item => item.Field != nameof(CocChangeField.Prices))
            .ToList();
        if (!nonPricePendingItems.Any())
        {
            logger.LogInformation("Creating a brand new request for LearningKey {0}", request.LearningKey);
            return new CocApprovalCommand
            {
                CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
                LearningKey = request.LearningKey,
                Action = AggregationAction.CreateNew,
                Prices = request.Prices
            };
        }

        var aggregated = Aggregate(request, nonPricePendingItems);
        request.Changes = aggregated.Items;
        if (aggregated.Items.Any())
        {
            logger.LogInformation("Creating a superseded request for LearningKey {0}", request.LearningKey);
            return new CocApprovalCommand
            {
                CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
                LearningKey = request.LearningKey,
                Action = AggregationAction.SupersedePrevious,
                Prices = request.Prices,
                ReversedFields = aggregated.ReversedFields
            };
        }

        logger.LogInformation("Reversing pending fields for LearningKey {0}", request.LearningKey);
        return new CocApprovalCommand
        {
            CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
            LearningKey = request.LearningKey,
            Action = AggregationAction.CreateNew,
            Prices = request.Prices,
            ReversedFields = aggregated.ReversedFields
        };
    }

    private async Task<List<CocApprovalItem>> GetPreviousPendingItemsForLearningKey(Guid learningKey)
    {
        var db = dbContext.Value;
        logger.LogInformation("Looking for previous pending requests for LearningKey {0}", learningKey);
        var pendingItems = await db.CocApprovalItems.Where(r => r.LearningKey == learningKey && r.Status == CocApprovalItemStatus.Pending).ToListAsync();
        return pendingItems;
    }

    public List<CocApprovalFieldChange> CreateAggregratedItems(CocApprovalRequest request, List<CocApprovalItem> previousItems)
    {
        return Aggregate(request, previousItems).Items;
    }

    private (List<CocApprovalFieldChange> Items, List<string> ReversedFields) Aggregate(CocApprovalRequest request, List<CocApprovalItem> previousItems)
    {
        var list = new List<CocApprovalFieldChange>();
        var reversedFields = new List<string>();

        foreach (var change in request.Changes ?? [])
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

                if (newChange.Data.Old == newChange.Data.New)
                {
                    logger.LogInformation("Change for field {Field} has no effective change, skipping", newChange.ChangeType);
                    reversedFields.Add(newChange.ChangeType);
                    continue;
                }
                list.Add(newChange);
            }
        }

        // TODO: Remove this commented loop once testing shows an omitted field should not be carried forward.
        //foreach (var item in previousItems)
        //{
        //    var newChange = request.Changes?.FirstOrDefault(c => c.ChangeType == item.Field);
        //    if (newChange == null)
        //    {
        //        list.Add(new CocApprovalFieldChange
        //        {
        //            ChangeType = item.Field,
        //            Data = new CocData
        //            {
        //                Old = item.Old,
        //                New = item.New
        //            }
        //        });
        //    }
        //}

        return (list, reversedFields);
    }
}