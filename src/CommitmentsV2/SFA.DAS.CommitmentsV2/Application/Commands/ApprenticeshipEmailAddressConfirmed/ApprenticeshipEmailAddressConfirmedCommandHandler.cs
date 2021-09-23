using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Models.Api.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ApprenticeshipEmailAddressConfirmed
{
    public class ApprenticeshipEmailAddressConfirmedCommandHandler : AsyncRequestHandler<ApprenticeshipEmailAddressConfirmedCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly IApiClient _apimClient;

        public ApprenticeshipEmailAddressConfirmedCommandHandler(Lazy<ProviderCommitmentsDbContext> db, IApiClient apimClient)
        {
            _db = db;
            _apimClient = apimClient;
        }

        protected override async Task Handle(ApprenticeshipEmailAddressConfirmedCommand request, CancellationToken cancellationToken)
        {
            var apprenticeshipTask =_db.Value.Apprenticeships.SingleAsync(a => a.Id == request.ApprenticeshipId, cancellationToken);
            var apprenticeTask = _apimClient.Get<ApprenticeResponse>(new GetApprentice(request.ApprenticeId));

            await Task.WhenAll(apprenticeTask, apprenticeshipTask);

            var apprenticeship = await apprenticeshipTask;
            var apprentice = await apprenticeTask;

            apprenticeship.ConfirmEmailAddress(apprentice.Email);
        }
    }
}