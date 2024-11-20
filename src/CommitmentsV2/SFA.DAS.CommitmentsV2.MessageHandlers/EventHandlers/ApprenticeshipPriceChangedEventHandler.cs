using SFA.DAS.Apprenticeships.Types;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ApprenticeshipPriceChangedEventHandler : IHandleMessages<ApprenticeshipPriceChangedEvent>
    {
        private readonly ILogger<ApprenticeshipPriceChangedEventHandler> _logger;
        private readonly IMediator _mediator;

        public ApprenticeshipPriceChangedEventHandler(
            ILogger<ApprenticeshipPriceChangedEventHandler> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(ApprenticeshipPriceChangedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("Received ApprenticeshipPriceChangedEvent for apprenticeshipId : {apprenticeshipId}", message.ApprenticeshipId);

            ResolveUsers(message, out var initiator, out var approver);

            await EditApprenticeship(message, initiator);
            await ApproveApprenticeship(message, approver);

            _logger.LogInformation("Successfully completed handling of {eventName}", nameof(ApprenticeshipPriceChangedEvent));
        }

        private async Task EditApprenticeship(ApprenticeshipPriceChangedEvent message, PartyUser partyUser)
        {
            var latestPrice = message.Episode.Prices.OrderBy(x => x.EndDate).Last();

            var editApprenticeshipRequest = new EditApprenticeshipApiRequest
            {
                ApprenticeshipId = message.ApprenticeshipId,
                AccountId = partyUser.AccountId,
                ProviderId = message.Episode.Ukprn,
                Cost = latestPrice.TrainingPrice + latestPrice.EndPointAssessmentPrice,
                TrainingPrice = latestPrice.TrainingPrice,
                EndPointAssessmentPrice = latestPrice.EndPointAssessmentPrice,
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

        private async Task ApproveApprenticeship(ApprenticeshipPriceChangedEvent message, PartyUser partyUser)
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
            ApprenticeshipPriceChangedEvent message, out PartyUser initiator, out PartyUser approver)
        {
            switch (message.ApprovedBy)
            {
                case Apprenticeships.Types.Enums.ApprovedBy.Provider:
                    initiator = new PartyUser(Party.Employer, message.Episode.EmployerAccountId, message.EmployerApprovedBy);
                    approver = new PartyUser(Party.Provider, message.Episode.Ukprn, message.ProviderApprovedBy);
                    break;

                case Apprenticeships.Types.Enums.ApprovedBy.Employer:
                    initiator = new PartyUser(Party.Provider, message.Episode.Ukprn, message.ProviderApprovedBy);
                    approver = new PartyUser(Party.Employer, message.Episode.EmployerAccountId, message.EmployerApprovedBy);
                    break;

                default:
                    throw new ArgumentException($"Invalid initiator {message.ApprovedBy}");
            }
        }
    }
}

