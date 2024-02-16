using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Notifications.Messages.Commands;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;

public class SendEmailToProviderCommandHandler : IHandleMessages<SendEmailToProviderCommand>
{
    private readonly IApprovalsOuterApiClient _approvalsOuterApiClient;
    private readonly ILogger<SendEmailToProviderCommandHandler> _logger;

    public SendEmailToProviderCommandHandler(IApprovalsOuterApiClient approvalsOuterApiClient, ILogger<SendEmailToProviderCommandHandler> logger)
    {
        _approvalsOuterApiClient = approvalsOuterApiClient;
        _logger = logger;
    }

    public async Task Handle(SendEmailToProviderCommand message, IMessageHandlerContext context)
    {
        try
        {
            var providerUsersResponse = await _approvalsOuterApiClient.Get<ProvidersUsersResponse>(new GetProviderUsersRequest(message.ProviderId));

            if (providerUsersResponse == null)
            {
                _logger.LogWarning($"No users found for ProviderId {message.ProviderId}");
                return;
            }

            var explicitEmailAddresses = string.IsNullOrWhiteSpace(message.EmailAddress)
                ? []
                : new List<string> { message.EmailAddress };

            List<string> recipients;

            if (explicitEmailAddresses.Any())
            {
                _logger.LogInformation("Explicit recipients requested for email");

                recipients = explicitEmailAddresses;
            }
            else
            {
                recipients = providerUsersResponse.Users.Any(u => !u.IsSuperUser) ?
                    providerUsersResponse.Users.Where(x => !x.IsSuperUser).Select(x => x.EmailAddress).ToList():
                    providerUsersResponse.Users.Select(x => x.EmailAddress).ToList();
            }

            var optedOutList = providerUsersResponse.Users.Where(x => !x.ReceiveNotifications).Select(x => x.EmailAddress).ToList();

            var finalRecipients = recipients
                .Where(x => !optedOutList.Exists(y => x.Equals(y, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();

            if (finalRecipients.Any())
            {
                _logger.LogInformation($"Calling SendEmailCommand for {finalRecipients.Count} emails");
                var tasks = finalRecipients.Select(email => context.Send(new SendEmailCommand(message.Template, email, message.Tokens)));
                await Task.WhenAll(tasks);

                _logger.LogInformation($"Sent email to {finalRecipients.Count} recipients for ukprn: {message.ProviderId}");
            }
            else
            {
                _logger.LogWarning($"No Email Addresses found to send Template {message.Template} for ProviderId {message.ProviderId}");
            }

        }
        catch (Exception e)
        {
            _logger.LogError($"Error processing {nameof(SendEmailToProviderCommand)}", e);
            throw;
        }
    }
}