using System.Globalization;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using UserInfo = SFA.DAS.CommitmentsV2.Types.UserInfo;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;

public class ProcessApprenticeshipApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
     IMessageSession messageSession)
    : IRequestHandler<ProcessApprenticeshipApprovalCommand>
{
    public async Task Handle(ProcessApprenticeshipApprovalCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

        var approval = await db.ApprovalRequests.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == command.ApprovalRequestId, cancellationToken);

        if (approval == null)
        {
            throw new Exception($"Approval request {command.ApprovalRequestId} not found");
        }
        if (approval.ApprenticeshipId != command.ApprenticeshipId)
        {
            throw new Exception($"Approval request {command.ApprovalRequestId} not found for apprenticeship {command.ApprenticeshipId}");
        }
        if (approval.Status != CocApprovalResultStatus.Pending)
        {
            throw new Exception($"Approval request {command.ApprovalRequestId} is no longer pending. It's status is {approval.Status}");
        }

        if(TotalPriceExceedsLimit(approval))
        {
            throw new DomainException("ApproveChanges", "The total cost must be £100,000 or less");
        }

        if(command.ApplyChanges)
        {
            approval.Status = CocApprovalResultStatus.Complete;
            foreach (var item in approval.Items)
            {
                item.Status = CocApprovalItemStatus.EmployerApproved;
                item.ApproverId = command.UserInfo?.UserId;
            }

            var approved = new LearningChangeApprovedEvent
            {
                LearningKey = approval.LearningKey,
                ApprenticeshipId = approval.ApprenticeshipId,
                Changes = ConvertItemsToChangeDictionary(approval.Items)
            };
            await messageSession.Publish(approved);
        }
        else
        {
            approval.Status = CocApprovalResultStatus.Complete;
            foreach (var item in approval.Items)
            {
                item.Status = CocApprovalItemStatus.EmployerRejected;
                item.ApproverId = command.UserInfo?.UserId;
            }

            var rejected = new LearningChangeRejectedEvent
            {
                LearningKey = approval.LearningKey,
                ApprenticeshipId = approval.ApprenticeshipId,
                Changes = ConvertItemsToChangeDictionary(approval.Items)
            };
            await messageSession.Publish(rejected);
        }

        await RecordCocUpdatesInLearnerHistory(approval, command.UserInfo, command.ApplyChanges);
    }

    private async Task RecordCocUpdatesInLearnerHistory(ApprovalRequest approval, UserInfo userInfo, bool applyChanges)
    {

        if(approval.Items != null && approval.Items.Any(x=>x.Field == "TNP1" || x.Field == "TNP2"))
        {
            var totalOldValues = SumStringList(approval.Items.Where(x => x.Field == "TNP1" || x.Field == "TNP2").Select(x => x.Old).ToList());
            var totalNewValues = SumStringList(approval.Items.Where(x => x.Field == "TNP1" || x.Field == "TNP2").Select(x => x.New).ToList());

            await messageSession.Send(new StoreLearningHistoryCommand
            {
                ApprenticeshipId = approval.ApprenticeshipId,
                Source = LearningSourceType.ApprovalAPI,
                ChangeType = applyChanges ? LearningChangeType.EmployerApproved : LearningChangeType.EmployerRejected,
                AppliedDate = DateTime.UtcNow,
                Description = $"Total price change from {ToCurrency(totalOldValues)} to {ToCurrency(totalNewValues)}",
                UserId = GetUserId(userInfo)
            });
        }
    }

    private bool TotalPriceExceedsLimit(ApprovalRequest approval)
    {
        if(approval.Items == null)
            return false;
        return SumStringList(approval.Items.Where(x => x.Field == "TNP1" || x.Field == "TNP2").Select(x => x.New).ToList()) > 100000;
    }

    public static string ToCurrency(int input)
    {
        var culture = new CultureInfo("en-GB");
        return input.ToString("C0", culture);
    }

    private int SumStringList(List<string> list)
    {
        int total = 0;

        foreach (var s in list)
        {
            if (int.TryParse(s, out int value))
                total += value;
        }
        return total;
    }

    private static Guid? GetUserId(UserInfo userInfo)
    {
        if (userInfo?.UserId != null && Guid.TryParse(userInfo.UserId, out var userId))
        {
            return userId;
        }

        return null;
    }

    private Dictionary<string, LearningChangeEvent.Change> ConvertItemsToChangeDictionary(ICollection<ApprovalFieldRequest> items)
    {
        var changes = new Dictionary<string, LearningChangeEvent.Change>();
        foreach (var item in items)
        {
            changes[MapToLearningFieldName(item.Field)] = new LearningChangeEvent.Change
            {
                Old = item.Old,
                New = item.New,
                EffectiveFromDate = item.EffectiveFromDate
            };
        }
        return changes;
    }

    private string MapToLearningFieldName(string fieldName)
    {
        return fieldName switch
        {
            "TNP1" => "TrainingPrice",
            "TNP2" => "AssessmentPrice",
            _ => fieldName
        };
    }
}