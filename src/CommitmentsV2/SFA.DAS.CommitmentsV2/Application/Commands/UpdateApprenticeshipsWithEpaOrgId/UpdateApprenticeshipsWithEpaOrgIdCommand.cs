using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId
{
    public class UpdateApprenticeshipsWithEpaOrgIdCommand: IRequest<long?>
    {
        public IEnumerable<SubmissionEvent> SubmissionEvents { get; private set; }
        public UpdateApprenticeshipsWithEpaOrgIdCommand(IEnumerable<SubmissionEvent> submissionEvents)
        {
            SubmissionEvents = submissionEvents;
        }
    }
}
