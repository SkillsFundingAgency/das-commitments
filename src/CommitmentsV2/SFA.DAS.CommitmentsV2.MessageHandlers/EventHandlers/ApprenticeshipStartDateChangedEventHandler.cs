using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipStartDateChangedEventHandler : IHandleMessages<ApprenticeshipStartDateChangedEvent>
    {
        private readonly ILogger<ApprenticeshipStartDateChangedEventHandler> _logger;
        private readonly IMediator _mediator;

        public ApprenticeshipStartDateChangedEventHandler(
            ILogger<ApprenticeshipStartDateChangedEventHandler> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(ApprenticeshipStartDateChangedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received ApprenticeshipStartDateChangedEvent for apprenticeshipId : {apprenticeshipId}", message.ApprenticeshipId);

            ResolveUsers(message, out var initiator, out var approver);

            await EditApprenticeship(message, initiator);
            await ApproveApprenticeship(message, approver);

            _logger.LogInformation("Successfully completed handling of {eventName}", nameof(ApprenticeshipStartDateChangedEvent));
        }

        private async Task EditApprenticeship(ApprenticeshipStartDateChangedEvent message, PartyUser partyUser)
        {
            var editApprenticeshipRequest = new EditApprenticeshipApiRequest
            {
                ApprenticeshipId = message.ApprenticeshipId,
                AccountId = partyUser.AccountId,
                ProviderId = message.ProviderId,
                StartDate = message.ActualStartDate,
                ActualStartDate = message.ActualStartDate,
                UserInfo = partyUser.UserInfo
            };

            var command = new EditApprenticeshipCommand(editApprenticeshipRequest, partyUser.Party);

            try
            {
                var response = await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending EditApprenticeshipCommand to mediator for apprenticeshipId : {apprenticeshipId}", message.ApprenticeshipId);
                throw;
            }
        }

        private async Task ApproveApprenticeship(ApprenticeshipStartDateChangedEvent message, PartyUser partyUser)
        {
            var command = new AcceptApprenticeshipUpdatesCommand(
                partyUser.Party, partyUser.AccountId, message.ApprenticeshipId, partyUser.UserInfo);

            try
            {
                await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending AcceptApprenticeshipUpdatesCommand to mediator for apprenticeshipId : {apprenticeshipId}", message.ApprenticeshipId);
                throw;
            }
        }

        private static void ResolveUsers(
            ApprenticeshipStartDateChangedEvent message, out PartyUser initiator, out PartyUser approver)
        {
            switch (message.Initiator)
            {
                case "Employer":
                    initiator = new PartyUser(Party.Employer, message.EmployerAccountId, message.EmployerApprovedBy);
                    approver = new PartyUser(Party.Provider, message.ProviderId, message.ProviderApprovedBy);
                    break;

                case "Provider":
                    initiator = new PartyUser(Party.Provider, message.ProviderId, message.ProviderApprovedBy);
                    approver = new PartyUser(Party.Employer, message.EmployerAccountId, message.EmployerApprovedBy);
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
                UserDisplayName = "PaymentSimplification",
                UserEmail = "payment@simplification.com"
            };
        }
    }
}

