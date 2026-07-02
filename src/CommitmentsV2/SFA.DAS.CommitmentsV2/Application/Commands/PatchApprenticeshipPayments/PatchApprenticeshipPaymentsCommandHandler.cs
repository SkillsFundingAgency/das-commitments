using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PatchApprenticeshipPayments;

public class PatchApprenticeshipPaymentsCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ICurrentDateTime currentDate,
    IAuthenticationService authenticationService,
    IMessageSession messageSession,
    IEncodingService encodingService,
    CommitmentsV2Configuration commitmentsV2Configuration)
    : IRequestHandler<PatchApprenticeshipPaymentsCommand>
{
    public async Task Handle(PatchApprenticeshipPaymentsCommand command, CancellationToken cancellationToken)
    {
        var isFreeze = command.PaymentFreezeDate.HasValue;
        var party = GetParty(command);

        CheckPartyIsValid(party, isFreeze);

        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

        if (isFreeze)
        {
            if (!command.FreezePaymentsReason.HasValue)
            {
                throw new DomainException(nameof(command.FreezePaymentsReason), "A reason for pausing payments must be provided");
            }

            apprenticeship.FreezePayments(currentDate, party, command.UserInfo, command.FreezePaymentsReason.Value);
        }
        else
        {
            apprenticeship.UnfreezePayments(currentDate, party, command.UserInfo);
        }

        await dbContext.Value.SaveChangesAsync(cancellationToken);

        await SendEmail(isFreeze, apprenticeship.Cohort.ProviderId, apprenticeship.Cohort.AccountLegalEntity.Name, apprenticeship.Id);

        await messageSession.Send(new StoreLearningHistoryCommand
        {
            ApprenticeshipId = command.ApprenticeshipId,
            Source = LearningSourceType.ApprovalAPI,
            ChangeType = LearningChangeType.ManualUpdate,
            AppliedDate = isFreeze
                ? apprenticeship.PaymentFreezeDate!.Value
                : currentDate.UtcNow,
            Description = isFreeze
                ? $"Payments paused - {command.FreezePaymentsReason!.Value.GetEnumDescription()}"
                : "Payments resumed",
            UserId = GetUserId(command.UserInfo)
        });
    }

    private Party GetParty(PatchApprenticeshipPaymentsCommand command)
    {
        if (command.Party != Party.None)
        {
            return command.Party;
        }

        return authenticationService.GetUserParty();
    }

    private static void CheckPartyIsValid(Party party, bool isFreeze)
    {
        if (party != Party.Employer)
        {
            var action = isFreeze ? "freeze" : "unfreeze";
            throw new DomainException(nameof(party), $"Only employers are allowed to {action} payments - {party} is invalid");
        }
    }

    private static Guid? GetUserId(UserInfo userInfo)
    {
        if (userInfo?.UserId != null && Guid.TryParse(userInfo.UserId, out var userId))
        {
            return userId;
        }

        return null;
    }

    private async Task SendEmail(bool isFreeze, long providerId, string employerName, long apprenticeshipId)
    {
        var encodedApprenticeshipId = encodingService.Encode(apprenticeshipId, EncodingType.ApprenticeshipId);

        var sendEmailToProviderCommand = new SendEmailToProviderCommand(
            providerId,
            isFreeze ? "ProviderApprenticeshipPaymentFrozenNotification " : "ProviderApprenticeshipPaymentUnfrozenNotification",
            new Dictionary<string, string>
            {
                {"employer_name", employerName},
                {
                    "link_to_mange_apprenticeships",
                    $"{commitmentsV2Configuration.ProviderCommitmentsBaseUrl}{providerId}/apprentices/{encodedApprenticeshipId}"
                },
                { "link_to_unsubscribe", $"{commitmentsV2Configuration.ProviderUrl.ProviderApprenticeshipServiceBaseUrl}notification-settings"  }
            });

        await messageSession.Send(sendEmailToProviderCommand);
    }


}
