using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressChangedByApprentice
{
    public class ApprenticeshipEmailAddressChangedByApprenticeCommandHandler : IRequestHandler<ApprenticeshipEmailAddressChangedByApprenticeCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IApprovalsOuterApiClient _apimClient;
        private readonly ILogger<ApprenticeshipEmailAddressChangedByApprenticeCommandHandler> _logger;

        public ApprenticeshipEmailAddressChangedByApprenticeCommandHandler(Lazy<ProviderCommitmentsDbContext> db, IApprovalsOuterApiClient apimClient, ILogger<ApprenticeshipEmailAddressChangedByApprenticeCommandHandler> logger)
        {
            _db = db;
            _apimClient = apimClient;
            _logger = logger;
        }

        public async Task Handle(ApprenticeshipEmailAddressChangedByApprenticeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Getting Apprenticeship {0}", request.ApprenticeshipId);
                var apprenticeshipTask = _db.Value.Apprenticeships.SingleAsync(a => a.Id == request.ApprenticeshipId, cancellationToken);

                _logger.LogInformation("Getting Apprentice details for apprentice {0}", request.ApprenticeId);
                var apprenticeTask = _apimClient.Get<ApprenticeResponse>(new GetApprenticeRequest(request.ApprenticeId));

                await Task.WhenAll(apprenticeTask, apprenticeshipTask);

                var apprenticeship = await apprenticeshipTask;
                var apprentice = await apprenticeTask;

                var status = apprenticeship.GetApprenticeshipStatus(DateTime.Now);

                if (status != ApprenticeshipStatus.Stopped && status != ApprenticeshipStatus.Completed)
                {
                    if (apprenticeship.EmailAddressConfirmed == null)
                    {
                        throw new DomainException("Email",
                            $"Email Address cannot be updated for {apprenticeship.Id} as it's not yet been confirmed");
                    }

                    _logger.LogInformation("Setting Email Address for apprenticeship {0}", request.ApprenticeshipId);
                    apprenticeship.ChangeEmailAddress(apprentice.Email);
                }
                else
                {
                    _logger.LogInformation($"ApprenticeshipEmailAddressChangedByApprenticeCommand not recorded as apprenticeshipId {request.ApprenticeshipId}, has ApprenticeshipStatus of {status.GetDescription()}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error recording ApprenticeshipEmailAddressChangedByApprenticeCommand for apprenticeshipId {request.ApprenticeshipId}", e);
                throw;
            }
        }
    }
}