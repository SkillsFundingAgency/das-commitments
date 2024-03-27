using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddEmptyCohortHandler : IRequestHandler<AddEmptyCohortCommand, AddCohortResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<AddEmptyCohortHandler> _logger;
    private readonly IEncodingService _encodingService;
    private readonly ICohortDomainService _cohortDomainService;

    public AddEmptyCohortHandler(
        Lazy<ProviderCommitmentsDbContext> dbContext,
        IEncodingService encodingService,
        ILogger<AddEmptyCohortHandler> logger,
        ICohortDomainService cohortDomainService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _cohortDomainService = cohortDomainService;
        _encodingService = encodingService;
    }

    public async Task<AddCohortResult> Handle(AddEmptyCohortCommand command, CancellationToken cancellationToken)
    {
        var db = _dbContext.Value;

        var cohort = await _cohortDomainService.CreateEmptyCohort(command.ProviderId,
            command.AccountId,
            command.AccountLegalEntityId,
            command.UserInfo,
            cancellationToken);

        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync(cancellationToken);

        //this encoding and re-save could be removed and put elsewhere
        cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved empty cohort. Provider: {ProviderId} Account-Legal-Entity:{AccountLegalEntityId} Commitment-Id:{CohortId}", command.ProviderId, command.AccountLegalEntityId, cohort.Id);

        var response = new AddCohortResult
        {
            Id = cohort.Id,
            Reference = cohort.Reference
        };

        return response;
    }
}