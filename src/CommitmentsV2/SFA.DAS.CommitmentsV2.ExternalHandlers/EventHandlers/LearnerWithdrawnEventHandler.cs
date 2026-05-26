using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;

public class LearnerWithdrawnEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IMediator mediator,
    ILogger<LearnerWithdrawnEventHandler> logger)
    : IHandleMessages<LearnerWithdrawnEvent>
{
    public async Task Handle(LearnerWithdrawnEvent message, IMessageHandlerContext context)
    {
        try
        {
            logger.LogInformation("LearnerWithdrawnEvent for ApprenticeshipId {ApprenticeshipId} with WithdrawnDate {WithdrawnDate} and " +
                "WithdrawnReasonCode {WithdrawnReasonCode}",
                message.ApprenticeshipId, message.WithdrawnDate, message.WithdrawnReasonCode);
            var db = dbContext.Value;
            var apprenticeship = await db.Apprenticeships
                .Include(x => x.Cohort)
                .SingleAsync(x => x.Id == message.ApprenticeshipId);

            await mediator.Send(new StopApprenticeshipCommand(
                apprenticeship.Cohort.EmployerAccountId,
                message.ApprenticeshipId,
                message.WithdrawnDate,
                false,
                UserInfo.System,
                Party.Employer,
                StopSource.Ilr,
                message.WithdrawnReasonCode,
                message.LearningKey,
                message.Created));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing LearnerWithdrawnEventHandler for ApprenticeshipId {0}", message.ApprenticeshipId);
            throw;
        }
    }
}

// Will be removed once Learning creates the message
public class LearnerWithdrawnEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public DateTime Created { get; set; }
    public DateTime WithdrawnDate { get; set; }
    public int WithdrawnReasonCode { get; set; }
}