using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.HashingService;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class CohortDomainService : ICohortDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private readonly ILogger<CohortDomainService> _logger;
        private readonly IUlnValidator _ulnValidator;

        public CohortDomainService(Lazy<ProviderCommitmentsDbContext> dbContext, ICurrentDateTime currentDateTime,
            ILogger<CohortDomainService> logger, IAcademicYearDateProvider academicYearDateProvider, IUlnValidator ulnValidator)
        {
            _dbContext = dbContext;
            _currentDateTime = currentDateTime;
            _logger = logger;
            _academicYearDateProvider = academicYearDateProvider;
            _ulnValidator = ulnValidator;
        }

        public async Task<Commitment> CreateCohort(long providerId, long accountLegalEntityId,
            DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            //get other aggregates and external data
            var provider = await db.Providers.SingleOrDefaultAsync(p => p.UkPrn == providerId, cancellationToken);
            if (provider == null) throw new BadRequestException($"Provider {providerId} was not found");
            var accountLegalEntity =
                await db.AccountLegalEntities.SingleOrDefaultAsync(x => x.Id == accountLegalEntityId,
                    cancellationToken);
            if (accountLegalEntity == null)
                throw new BadRequestException($"AccountLegalEntity {accountLegalEntityId} was not found");

            var cohort = provider.CreateCohort(accountLegalEntity, draftApprenticeshipDetails);

            ValidateDraftApprenticeshipDetails(draftApprenticeshipDetails);

            return cohort;
        }

        private void ValidateDraftApprenticeshipDetails(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            var errors = new List<DomainError>();
            errors.AddRange(BuildStartDateValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildUlnValidationFailures(draftApprenticeshipDetails));
            //overlap check
            //reservation validation
            errors.ThrowIfAny();
        }


        private IEnumerable<DomainError> BuildUlnValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (!string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln))
            {
                var validationResult = _ulnValidator.Validate(draftApprenticeshipDetails.Uln);
                switch (validationResult)
                {
                    case UlnValidationResult.IsInValidTenDigitUlnNumber:
                        yield return new DomainError(nameof(draftApprenticeshipDetails.Uln), "You must enter a 10-digit unique learner number");
                        yield break;
                    case UlnValidationResult.IsInvalidUln:
                        yield return new DomainError(nameof(draftApprenticeshipDetails.Uln), "You must enter a valid unique learner number");
                        yield break;
                    default:
                        yield break;
                }
            }
        }

        private IEnumerable<DomainError> BuildStartDateValidationFailures(DraftApprenticeshipDetails details)
        {
            if (!details.StartDate.HasValue)
            {
                yield break;
            }
            
            if (details.StartDate.Value > _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
            {
                yield return new DomainError(nameof(details.StartDate),
                        "The start date must be no later than one year after the end of the current teaching year");
            }
        }
    }
}
