namespace SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId
{
    public class AddLastSubmissionEventIdCommand : IRequest
    {
        public long LastSubmissionEventId { get; private set; }
        public AddLastSubmissionEventIdCommand(long lastSubmissionEventId)
        {
            LastSubmissionEventId = lastSubmissionEventId;
        }
    }
}
