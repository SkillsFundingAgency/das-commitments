using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects.Reservations;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class CohortDomainService : ICohortDomainService
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private readonly ILogger<CohortDomainService> _logger;
        private readonly IUlnValidator _ulnValidator;
        private readonly IReservationValidationService _reservationValidationService;

        public CohortDomainService(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<CohortDomainService> logger,
            IAcademicYearDateProvider academicYearDateProvider,
            IUlnValidator ulnValidator,
            IReservationValidationService reservationValidationService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _academicYearDateProvider = academicYearDateProvider;
            _ulnValidator = ulnValidator;
            _reservationValidationService = reservationValidationService;
        }

        public async Task<Cohort> CreateCohort(long providerId, long accountLegalEntityId,
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

            await ValidateDraftApprenticeshipDetails(providerId, accountLegalEntity.AccountId, accountLegalEntity.PublicHashedId, draftApprenticeshipDetails, cancellationToken);

            return cohort;
        }

        private async Task ValidateDraftApprenticeshipDetails(long providerId, long accountId, string accountLegalEntityPublicHashedId, DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken)
        {
            var errors = new List<DomainError>();
            errors.AddRange(BuildStartDateValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildUlnValidationFailures(draftApprenticeshipDetails));
            //overlap check to go here
            errors.AddRange(await BuildReservationValidationFailures(providerId, accountId, accountLegalEntityPublicHashedId, draftApprenticeshipDetails, cancellationToken));
            errors.ThrowIfAny();
        }


        private IEnumerable<DomainError> BuildUlnValidationFailures(
            DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (!string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln))
            {
                var validationResult = _ulnValidator.Validate(draftApprenticeshipDetails.Uln);
                switch (validationResult)
                {
                    case UlnValidationResult.IsInValidTenDigitUlnNumber:
                        yield return new DomainError(nameof(draftApprenticeshipDetails.Uln),
                            "You must enter a 10-digit unique learner number");
                        yield break;
                    case UlnValidationResult.IsInvalidUln:
                        yield return new DomainError(nameof(draftApprenticeshipDetails.Uln),
                            "You must enter a valid unique learner number");
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

        private async Task<IEnumerable<DomainError>> BuildReservationValidationFailures(long providerId, long accountId, string accountLegalEntityPublicHashedId, DraftApprenticeshipDetails details, CancellationToken cancellationToken)
        {
            if (!details.ReservationId.HasValue)
            {
                return new DomainError[0];
            }

            var validationRequest = new ReservationValidationRequest(providerId, accountId,
                accountLegalEntityPublicHashedId, details.ReservationId.Value, details.StartDate, details.TrainingProgramme?.CourseCode);

            var validationResult = await _reservationValidationService.Validate(validationRequest, cancellationToken);

            return validationResult.ValidationErrors.Select(error => new DomainError(error.PropertyName, error.Reason)).ToList();
        }
    }
}
