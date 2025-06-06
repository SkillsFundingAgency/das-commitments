﻿using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation;

public class ResendInvitationCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService,
    IMessageSession messageSession,
    ILogger<ResendInvitationCommandHandler> logger)
    : IRequestHandler<ResendInvitationCommand>
{
    public async Task Handle(ResendInvitationCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var party = authenticationService.GetUserParty();
            var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

            if (apprenticeship.Email == null)
            {
                throw new DomainException("Email", "Invitation cannot be sent as there is no email associated with apprenticeship");
            }

            if (apprenticeship.EmailAddressConfirmed == true)
            {
                throw new DomainException("Email", "Email address has been confirmed");
            }

            await messageSession.Send(new ApprenticeshipResendInvitationCommand
            {
                ApprenticeshipId = apprenticeship.Id,
                ResendOn = currentDate.UtcNow
            });
            
            logger.LogInformation("Resending Invitation for Apprenticeship id {ApprenticeshipId}, initiated by {Party} : userId {UserId}", command.ApprenticeshipId, party, command.UserInfo.UserId);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error Resending Invitation for Apprenticeship id {ApprenticeshipId}", command.ApprenticeshipId);
            throw;
        }
    }
}