using MediatR;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CompleteTask
{
    public class CompleteTaskCommand : IAsyncRequest
    {
        public long ProviderId { get; set; }
        public long TaskId { get; set; }
    }
}