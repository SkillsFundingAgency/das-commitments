using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort
{
    public class ProcessFullyApprovedCohortCommand : IRequest
    {
        public long CohortId { get; }
        public long AccountId { get; }
        public long? ChangeOfPartyRequestId { get; set; }

        public ProcessFullyApprovedCohortCommand(long cohortId, long accountId, long? changeOfPartyRequestId)
        {
            CohortId = cohortId;
            AccountId = accountId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
        }
    }
}