using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;

public class PostCocApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICocApprovalService cocApprovalService,
    ILogger<PostCocApprovalCommandHandler> logger)
    : IRequestHandler<PostCocApprovalCommand, CocApprovalResult>
{
    public async Task<CocApprovalResult> Handle(PostCocApprovalCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("=== COMMITMENTS API: PostCocApprovalCommandHandler.Handle called ===");

        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        var db = dbContext.Value;
        var existingApprovalRequests = db.ApprovalRequests.Where(r => r.LearningKey == command.LearningKey && r.Status == CocApprovalRequestStatus.Pending);

        if (existingApprovalRequests.Any())
        {
            throw new DomainException("LearningKey", "An approval request for this learning key already exists.");
        }

        var approvalRequestStatus = cocApprovalService.DetermineAndSetCocApprovalStatuses(command.Changes, command.Apprenticeship);

        var approvalRequest = new ApprovalRequest
        {
            LearningKey = command.LearningKey,
            ApprenticeshipId = command.ApprenticeshipId,
            LearningType = command.LearningType,
            UKPRN = command.ProviderId.ToString(),
            ULN = command.ULN,
            Status = approvalRequestStatus,
            Items = command.ApprovalFieldChanges.Select(change => MapTo(change, command)).ToList()
        };

        db.ApprovalRequests.Add(approvalRequest);

        return CreateCocApprovalResult(approvalRequest);
    }

    private CocApprovalResult CreateCocApprovalResult(ApprovalRequest approvalRequest)
    {
        return new CocApprovalResult
        {
            Status = approvalRequest.Status,
            Items = approvalRequest.Items.Select(item => new CocApprovalItemResult
            {
                ChangeType = item.Field,
                Status = item.Status?.GetEnumDescription()
            }).ToList()
        };
    }

    private ApprovalFieldRequest MapTo(CocApprovalFieldChange field, PostCocApprovalCommand command)
    {
        return new ApprovalFieldRequest
        {
            Field = field.ChangeType,
            Old = field.Data.Old,
            New = field.Data.New,
            Status = GetStatusForChange(field.ChangeType, command.Changes)
        };

    }

    private CocApprovalItemStatus? GetStatusForChange(object changeType, CocChanges changes)
    {
        switch (changeType)
        {
            case "TNP1":
                return changes.TNP1.Status;
            case "TNP2":
                return changes.TNP2.Status;
        }
        return null;
    }
}