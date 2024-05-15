using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId
{
    public class UpdateApprenticeshipsWithEpaOrgIdCommand: IRequest<long?>
    {
        public IEnumerable<SubmissionEvent> SubmissionEvents { get; private set; }
        public UpdateApprenticeshipsWithEpaOrgIdCommand(IEnumerable<SubmissionEvent> submissionEvents)
        {
            SubmissionEvents = submissionEvents;
        }

        public UpdateApprenticeshipsWithEpaOrgIdCommand()
        {
        }
    }
}
