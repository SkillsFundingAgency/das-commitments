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
        var previousApprovalRequest = await GetPreviousPendingApprovalRequestForLearningKey(request.LearningKey);
        if (previousApprovalRequest == null)
        {
            logger.LogInformation("Creating a brand new request for LearningKey {0}", request.LearningKey);
            return new CocApprovalCommand
            {
                CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
                Action = AggregrationAction.CreateNew,
                PreviousApprovalRequestId = null
            };
        }

        // Should we validate that the ApprenticeshipId and other fields match between the previous request and the new request?
        // Thjere is nothing in the requirements that says we should, but it might be a good idea to ensure that the new request
        // is for the same apprenticeship and learning type as the previous request.

        var aggregatedItems = CreateAggregratedItems(request, previousApprovalRequest);

        if (aggregatedItems.Any())
        {
            logger.LogInformation("Creating a superseded request for LearningKey {0}", request.LearningKey);

            request.Changes = aggregatedItems;
            return new CocApprovalCommand
            {
                CocApprovalDetails = await modelMapper.Map<CocApprovalDetails>(request),
                Action = AggregrationAction.SupersedePrevious,
                PreviousApprovalRequestId = previousApprovalRequest.Id
            };
        }

        logger.LogInformation("Cancelling the pending request {0} for LearningKey {1}", previousApprovalRequest.Id, request.LearningKey);
        return new CocApprovalCommand
        {
            CocApprovalDetails = null,
            Action = AggregrationAction.CancelPrevious,
            PreviousApprovalRequestId = previousApprovalRequest.Id
        };
    }

    public async Task<ApprovalRequest> GetPreviousPendingApprovalRequestForLearningKey(Guid learningKey)
    {
        logger.LogInformation("Looking for previous requests for LearningKey {0}", learningKey);
        var existingApprovalRequest = await dbContext.Value.ApprovalRequests.Include(r => r.Items).FirstOrDefaultAsync(r => r.LearningKey == learningKey && r.Status == CocApprovalResultStatus.Pending);
        return existingApprovalRequest;
    }

    public List<CocApprovalFieldChange> CreateAggregratedItems(CocApprovalRequest request, ApprovalRequest previousRequest)
    {
        var list = new List<CocApprovalFieldChange>();

        // Aggregate the changes from the new request with the previous request, 
        // keep new fields as they are, and for existing fields,
        // get the old value from the previous request and the new value from the new request.
        foreach (var change in request.Changes)
        {
            var previousChange = previousRequest.Items.FirstOrDefault(i => i.Field == change.ChangeType);
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
                        New = change.Data.New,
                        EffectiveFromDate = change.Data.EffectiveFromDate // We are taking the EffectiveFromDate from the new request, as it is the most recent change.
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
        foreach (var item in previousRequest.Items)
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
                        New = item.New,
                        EffectiveFromDate = item.EffectiveFromDate
                    }
                };
                list.Add(change);
            }
        }

        return list;
    }
}