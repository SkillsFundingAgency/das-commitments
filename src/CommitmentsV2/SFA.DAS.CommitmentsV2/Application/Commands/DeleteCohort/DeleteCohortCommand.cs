using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort
{
    public class DeleteCohortCommand : IRequest
    {
        public long CohortId { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}
