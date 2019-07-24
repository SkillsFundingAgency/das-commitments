using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortWithOtherPartyHandler : IRequestHandler<AddCohortWithOtherPartyCommand, AddCohortResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<AddCohortWithOtherPartyHandler> _logger;
        private readonly IEncodingService _encodingService;
        private readonly ICohortDomainService _cohortDomainService;

        public AddCohortWithOtherPartyHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IEncodingService encodingService,
            ILogger<AddCohortWithOtherPartyHandler> logger,
            IMapper<AddCohortCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _cohortDomainService = cohortDomainService;
            _encodingService = encodingService;
        }

        public async Task<AddCohortResponse> Handle(AddCohortWithOtherPartyCommand command, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var cohort = await _cohortDomainService.CreateCohort(command.ProviderId,
                command.AccountLegalEntityId,
                false,
                string.Empty,
                command.UserInfo,
                cancellationToken);

            db.Cohorts.Add(cohort);
            await db.SaveChangesAsync(cancellationToken);

            //this encoding and re-save could be removed and put elsewhere
            cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Saved cohort. Provider: {command.ProviderId} Account-Legal-Entity:{command.AccountLegalEntityId} Reservation-Id:{command.ReservationId} Commitment-Id:{cohort?.Id} Apprenticeship:{cohort?.Apprenticeships?.FirstOrDefault()?.Id}");

            var response = new AddCohortResponse
            {
                Id = cohort.Id,
                Reference = cohort.Reference
            };

            return response;
        }
    }
}
