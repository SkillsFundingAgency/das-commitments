using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortWithOtherPartyHandler : IRequestHandler<AddCohortWithOtherPartyCommand, AddCohortResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<AddCohortWithOtherPartyHandler> _logger;
        private readonly IEncodingService _encodingService;
        private readonly ICohortDomainService _cohortDomainService;

        public AddCohortWithOtherPartyHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IEncodingService encodingService,
            ILogger<AddCohortWithOtherPartyHandler> logger,
            ICohortDomainService cohortDomainService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _cohortDomainService = cohortDomainService;
            _encodingService = encodingService;
        }

        public async Task<AddCohortResult> Handle(AddCohortWithOtherPartyCommand command, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var cohort = await _cohortDomainService.CreateCohortWithOtherParty(command.ProviderId,
                command.AccountId,
                command.AccountLegalEntityId,
                command.TransferSenderId,
                command.PledgeApplicationId,
                command.Message,
                command.UserInfo,
                cancellationToken);

            db.Cohorts.Add(cohort);
            await db.SaveChangesAsync(cancellationToken);

            //this encoding and re-save could be removed and put elsewhere
            cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Saved cohort with other party. Provider: {command.ProviderId} Account-Legal-Entity:{command.AccountLegalEntityId} Commitment-Id:{cohort.Id}");

            var response = new AddCohortResult
            {
                Id = cohort.Id,
                Reference = cohort.Reference
            };

            return response;
        }
    }
}
