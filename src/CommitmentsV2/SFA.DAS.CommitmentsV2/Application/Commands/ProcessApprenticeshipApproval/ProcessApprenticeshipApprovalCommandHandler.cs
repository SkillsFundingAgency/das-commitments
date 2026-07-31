using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Threading.Channels;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessApprenticeshipApproval;

public class ProcessApprenticeshipApprovalCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext)
    : IRequestHandler<ProcessApprenticeshipApprovalCommand>
{
    public async Task Handle(ProcessApprenticeshipApprovalCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

        var approval = await db.ApprovalRequests.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == command.ApprovalRequestId, cancellationToken);

        if (approval == null)
        {
            throw new Exception($"Approval request {command.ApprovalRequestId} not found ");
        }
        if (approval.ApprenticeshipId != command.ApprenticeshipId)
        {
            throw new Exception($"Approval request {command.ApprovalRequestId} not found for apprenticeship {command.ApprenticeshipId}");
        }
        if (approval.Status != CocApprovalResultStatus.Pending)
        {
            throw new Exception($"Approval request {command.ApprovalRequestId} is no longer pending. It#s status is {approval.Status}");
        }

        if (command.ApplyChanges)
        {
            approval.Status = CocApprovalResultStatus.Complete;

        }
        else
        {
            approval.Status = CocApprovalResultStatus.Cancelled;
        }

        await db.SaveChangesAsync(cancellationToken);



    }

    private void SendAppliedMessage(ApprovalRequest approvalRequest)
    {

        var jsonObject = new
        {
            approvalRequest.LearningKey,
            approvalRequest.ApprenticeshipId,

            Changes = approvalRequest.Items.ToDictionary(
                x => x.Field,
                x => new
                {
                    x.Old,
                    x.New,
                    x.EffectiveFromDate
                })

        };
        var json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);




    }
}