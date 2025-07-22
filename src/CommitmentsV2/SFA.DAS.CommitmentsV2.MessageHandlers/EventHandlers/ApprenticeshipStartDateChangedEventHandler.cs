using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ApprenticeshipStartDateChangedEventHandler(
    ILogger<ApprenticeshipStartDateChangedEventHandler> logger,
    IMediator mediator)
    : IHandleMessages<LearningStartDateChangedEvent>
{
    public async Task Handle(LearningStartDateChangedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("Received ApprenticeshipStartDateChangedEvent for apprenticeshipId : {ApprenticeshipId}", message.ApprovalsApprenticeshipId);

        ResolveUsers(message, out var initiator, out var approver);

        await EditApprenticeship(message, initiator);
        await ApproveApprenticeship(message, approver);

        logger.LogInformation("Successfully completed handling of {EventName}", nameof(LearningStartDateChangedEvent));
    }

    private async Task EditApprenticeship(LearningStartDateChangedEvent message, PartyUser partyUser)
    {
        var editApprenticeshipRequest = new EditApprenticeshipApiRequest
        {
            ApprenticeshipId = message.ApprovalsApprenticeshipId,
            AccountId = partyUser.AccountId,
            ProviderId = message.Episode.Ukprn,
            StartDate = new DateTime(message.StartDate.Year, message.StartDate.Month, 1),
            EndDate = message.Episode.Prices.OrderBy(x => x.EndDate).Last().EndDate,
            ActualStartDate = message.StartDate,
            UserInfo = partyUser.UserInfo
        };

        var command = new EditApprenticeshipCommand(editApprenticeshipRequest, partyUser.Party);

        try
        {
            await mediator.Send(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending EditApprenticeshipCommand to mediator for apprenticeshipId : {ApprenticeshipId}", message.ApprovalsApprenticeshipId);
            throw;
        }
    }

    private async Task ApproveApprenticeship(LearningStartDateChangedEvent message, PartyUser partyUser)
    {
        var command = new AcceptApprenticeshipUpdatesCommand(partyUser.Party, partyUser.AccountId, message.ApprovalsApprenticeshipId, partyUser.UserInfo);

        try
        {
            await mediator.Send(command);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending AcceptApprenticeshipUpdatesCommand to mediator for apprenticeshipId : {ApprenticeshipId}", message.ApprovalsApprenticeshipId);
            throw;
        }
    }

    private static void ResolveUsers(LearningStartDateChangedEvent message, out PartyUser initiator, out PartyUser approver)
    {
        switch (message.Initiator)
        {
            case "Employer":
                initiator = new PartyUser(Party.Employer, message.Episode.EmployerAccountId, message.EmployerApprovedBy);
                approver = new PartyUser(Party.Provider, message.Episode.Ukprn, message.ProviderApprovedBy);
                break;

            case "Provider":
                initiator = new PartyUser(Party.Provider, message.Episode.Ukprn, message.ProviderApprovedBy);
                approver = new PartyUser(Party.Employer, message.Episode.EmployerAccountId, message.EmployerApprovedBy);
                break;

            default:
                throw new ArgumentException($"Invalid initiator {message.Initiator}");
        }
    }
}

public class PartyUser
{
    public Party Party { get; }
    public long AccountId { get; set; }
    public UserInfo UserInfo { get; }

    public PartyUser(Party party, long accountId, string userId)
    {
        Party = party;
        AccountId = accountId;
        UserInfo = new UserInfo
        {
            UserId = userId,
            UserDisplayName = "SYSTEM",
            UserEmail = "SYSTEM"
        };
    }
}