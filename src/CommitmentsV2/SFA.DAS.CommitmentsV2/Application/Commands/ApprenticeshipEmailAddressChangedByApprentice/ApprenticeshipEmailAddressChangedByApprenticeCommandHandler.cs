using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice;

public class ApprenticeshipEmailAddressChangedByApprenticeCommandHandler(
    Lazy<ProviderCommitmentsDbContext> db,
    IApprovalsOuterApiClient apimClient,
    ILogger<ApprenticeshipEmailAddressChangedByApprenticeCommandHandler> logger)
    : IRequestHandler<ApprenticeshipEmailAddressChangedByApprenticeCommand>
{
    public async Task Handle(ApprenticeshipEmailAddressChangedByApprenticeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Getting Apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
            var apprenticeshipTask = db.Value.Apprenticeships.SingleAsync(a => a.Id == request.ApprenticeshipId, cancellationToken);

            logger.LogInformation("Getting Apprentice details for apprentice {ApprenticeshipId}", request.ApprenticeId);
            var apprenticeTask = apimClient.Get<ApprenticeResponse>(new GetApprenticeRequest(request.ApprenticeId));

            await Task.WhenAll(apprenticeTask, apprenticeshipTask);

            var apprenticeship = await apprenticeshipTask;
            var apprentice = await apprenticeTask;

            var status = apprenticeship.GetApprenticeshipStatus(DateTime.Now);

            if (status != ApprenticeshipStatus.Stopped && status != ApprenticeshipStatus.Completed)
            {
                if (apprenticeship.EmailAddressConfirmed == null)
                {
                    throw new DomainException("Email", $"Email Address cannot be updated for {apprenticeship.Id} as it's not yet been confirmed");
                }

                logger.LogInformation("Setting Email Address for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
                apprenticeship.ChangeEmailAddress(apprentice.Email);
            }
            else
            {
                logger.LogInformation("ApprenticeshipEmailAddressChangedByApprenticeCommand not recorded as apprenticeshipId {ApprenticeshipId}, has ApprenticeshipStatus of {Description}", request.ApprenticeshipId, status.GetDescription());
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error recording ApprenticeshipEmailAddressChangedByApprenticeCommand for apprenticeshipId {ApprenticeshipId}", request.ApprenticeshipId);
            throw;
        }
    }
}