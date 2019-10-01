using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort
{
    public class ProcessFullyApprovedCohortCommand : IRequest
    {
        public long CohortId { get; }
        public long AccountId { get; }

        public ProcessFullyApprovedCohortCommand(long cohortId, long accountId)
        {
            CohortId = cohortId;
            AccountId = accountId;
        }
    }
}