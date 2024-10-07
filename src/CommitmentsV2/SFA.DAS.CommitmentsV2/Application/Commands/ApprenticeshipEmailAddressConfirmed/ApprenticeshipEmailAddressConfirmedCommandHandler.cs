using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;

public class ApprenticeshipEmailAddressConfirmedCommandHandler(Lazy<ProviderCommitmentsDbContext> db, IApprovalsOuterApiClient apimClient, ILogger<ApprenticeshipEmailAddressConfirmedCommandHandler> logger)
    : IRequestHandler<ApprenticeshipEmailAddressConfirmedCommand>
{
    public async Task Handle(ApprenticeshipEmailAddressConfirmedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var apprenticeshipTask = db.Value.Apprenticeships.SingleAsync(a => a.Id == request.ApprenticeshipId, cancellationToken);
            var apprenticeTask = apimClient.Get<ApprenticeResponse>(new GetApprenticeRequest(request.ApprenticeId));
            
            await Task.WhenAll(apprenticeshipTask, apprenticeTask);
            
            var apprenticeship = apprenticeshipTask.Result;
            var apprentice =  apprenticeTask.Result;
            
            logger.LogInformation("Setting Email Address for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
            
            apprenticeship.ConfirmEmailAddress(apprentice.Email);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error recording ApprenticeshipEmailAddressConfirmed for apprenticeshipId {ApprenticeshipId}", request.ApprenticeshipId);
            throw;
        }
    }
}