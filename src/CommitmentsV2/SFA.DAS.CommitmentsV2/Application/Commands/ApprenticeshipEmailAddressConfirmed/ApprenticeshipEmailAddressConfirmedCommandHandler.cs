using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed;

public class ApprenticeshipEmailAddressConfirmedCommandHandler : IRequestHandler<ApprenticeshipEmailAddressConfirmedCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _db;
    private readonly IApprovalsOuterApiClient _apimClient;
    private readonly ILogger<ApprenticeshipEmailAddressConfirmedCommandHandler> _logger;

    public ApprenticeshipEmailAddressConfirmedCommandHandler(Lazy<ProviderCommitmentsDbContext> db, IApprovalsOuterApiClient apimClient, ILogger<ApprenticeshipEmailAddressConfirmedCommandHandler> logger)
    {
        _db = db;
        _apimClient = apimClient;
        _logger = logger;
    }

    public async Task Handle(ApprenticeshipEmailAddressConfirmedCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var apprenticeshipTask = _db.Value.Apprenticeships.SingleAsync(a => a.Id == request.ApprenticeshipId, cancellationToken);

            var apprenticeTask = _apimClient.Get<ApprenticeResponse>(new GetApprenticeRequest(request.ApprenticeId));

            _logger.LogInformation("Getting Apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
            
            var apprenticeship = await apprenticeshipTask;
            
            _logger.LogInformation("Getting Apprentice details for apprentice {ApprenticeshipId}", request.ApprenticeId);
            
            var apprentice = await apprenticeTask;
            
            _logger.LogInformation("Setting Email Address for apprenticeship {ApprenticeshipId}", request.ApprenticeshipId);
            
            apprenticeship.ConfirmEmailAddress(apprentice.Email);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error recording ApprenticeshipEmailAddressConfirmed for apprenticeshipId {ApprenticeshipId}", request.ApprenticeshipId);
            throw;
        }
    }
}