using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

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
		_logger.LogInformation("Received ApprenticeshipStartDateChangedEvent for apprenticeshipId : " + message.ApprenticeshipId);

		await EditApprenticeship(message);


		var foo = "DO NOT APPROVE";
		_logger.LogInformation($"Sending {foo} for apprenticeshipId : {message.ApprenticeshipId}");
	}

	private async Task EditApprenticeship(ApprenticeshipStartDateChangedEvent message)
	{
		var editApprenticeshipRequest = new EditApprenticeshipApiRequest
		{
			ApprenticeshipId = message.ApprenticeshipId,
			AccountId = message.EmployerAccountId,
			ProviderId = message.ProviderId,
			StartDate = message.ActualStartDate,
			UserInfo = new UserInfo { UserId = message.UserId, UserDisplayName = message.UserDisplayName, UserEmail = message.UserEmail }
		};

		var party = Party.None;
		switch(message.Initiator)
		{
			case "Employer":
				party = Party.Employer;
				break;
			case "Provider":
				party = Party.Provider;
				break;
			default:
				throw new ArgumentException($"Invalid initiator {message.Initiator}");
		}

		var command = new EditApprenticeshipCommand(editApprenticeshipRequest, party);

		try
		{
			var response = await _mediator.Send(command);
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, $"Error sending EditApprenticeshipCommand to mediator for apprenticeshipId : {message.ApprenticeshipId}");
		}
	}
}
