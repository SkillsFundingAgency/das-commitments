using Azure.Core;
using SFA.DAS.CommitmentsV2.Application.Commands.ProcessFullyApprovedCohort;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class CohortFullyApprovedEventHandler(IMediator mediator, ILogger<CohortFullyApprovedEventHandler> logger) : IHandleMessages<CohortFullyApprovedEvent>
{
    public async Task Handle(CohortFullyApprovedEvent message, IMessageHandlerContext context)
    {
        try
        {
            await mediator.Send(new ProcessFullyApprovedCohortCommand(message.CohortId, message.AccountId, message.ChangeOfPartyRequestId, message.UserInfo, message.LastApprovedBy));
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error processing ProcessFullyApprovedCohortCommand for Cohort {CohortId}.", message.CohortId);
            throw;
        }
    }
}