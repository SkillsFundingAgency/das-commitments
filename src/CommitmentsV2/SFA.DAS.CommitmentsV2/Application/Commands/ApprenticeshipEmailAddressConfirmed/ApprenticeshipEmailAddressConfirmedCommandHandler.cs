using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Models.Api.Types;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed
{
    public class ApprenticeshipEmailAddressConfirmedCommandHandler : AsyncRequestHandler<ApprenticeshipEmailAddressConfirmedCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IApiClient _apimClient;
        private readonly ILogger<ApprenticeshipEmailAddressConfirmedCommandHandler> _logger;

        public ApprenticeshipEmailAddressConfirmedCommandHandler(Lazy<ProviderCommitmentsDbContext> db, IApiClient apimClient, ILogger<ApprenticeshipEmailAddressConfirmedCommandHandler> logger)
        {
            _db = db;
            _apimClient = apimClient;
            _logger = logger;
        }

        protected override async Task Handle(ApprenticeshipEmailAddressConfirmedCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var apprenticeshipTask = _db.Value.Apprenticeships.SingleAsync(a => a.Id == request.ApprenticeshipId, cancellationToken);
                var apprenticeTask = _apimClient.Get<ApprenticeResponse>(new GetApprenticeRequest(request.ApprenticeId));

                await Task.WhenAll(apprenticeTask, apprenticeshipTask);

                var apprenticeship = await apprenticeshipTask;
                var apprentice = await apprenticeTask;

                var status = apprenticeship.GetApprenticeshipStatus(DateTime.Now);

                if (status != ApprenticeshipStatus.Stopped && status != ApprenticeshipStatus.Completed)
                {
                    apprenticeship.ConfirmEmailAddress(apprentice.Email);
                }
                else
                {
                    _logger.LogInformation($"ApprenticeshipEmailAddressConfirmed not recorded as apprenticeshipId {request.ApprenticeshipId}, has ApprenticeshipStatus of {status.GetDescription()}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error recording ApprenticeshipEmailAddressConfirmed for apprenticeshipId {request.ApprenticeshipId}", e);
                throw;
            }
        }
    }
}