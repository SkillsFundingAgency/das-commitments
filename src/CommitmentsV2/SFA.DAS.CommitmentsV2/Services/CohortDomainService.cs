using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
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
        private readonly IOverlapCheckService _overlapCheckService;

        public CohortDomainService(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<CohortDomainService> logger,
            IAcademicYearDateProvider academicYearDateProvider,
            IUlnValidator ulnValidator,
            IReservationValidationService reservationValidationService,
            IOverlapCheckService overlapCheckService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _academicYearDateProvider = academicYearDateProvider;
            _ulnValidator = ulnValidator;
            _reservationValidationService = reservationValidationService;
            _overlapCheckService = overlapCheckService;
        }

        public async Task<Cohort> CreateCohort(long providerId, long accountLegalEntityId,
            DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var provider = await GetProvider(providerId, db, cancellationToken);
            var accountLegalEntity = await GetAccountLegalEntity(accountLegalEntityId, db, cancellationToken);

            var cohort = provider.CreateCohort(accountLegalEntity, draftApprenticeshipDetails);

            await ValidateDraftApprenticeshipDetails(providerId, accountLegalEntity.AccountId, accountLegalEntity.PublicHashedId, draftApprenticeshipDetails, cancellationToken);

            return cohort;
        }

        private static async Task<AccountLegalEntity> GetAccountLegalEntity(long accountLegalEntityId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
        {
            var accountLegalEntity =
                await db.AccountLegalEntities.SingleOrDefaultAsync(x => x.Id == accountLegalEntityId,
                    cancellationToken);
            if (accountLegalEntity == null)
                throw new BadRequestException($"AccountLegalEntity {accountLegalEntityId} was not found");
            return accountLegalEntity;
        }

        private static async Task<Provider> GetProvider(long providerId, ProviderCommitmentsDbContext db, CancellationToken cancellationToken)
        {
            var provider = await db.Providers.SingleOrDefaultAsync(p => p.UkPrn == providerId, cancellationToken);
            if (provider == null) throw new BadRequestException($"Provider {providerId} was not found");
            return provider;
        }

        private async Task ValidateDraftApprenticeshipDetails(long providerId, long accountId, string accountLegalEntityPublicHashedId, DraftApprenticeshipDetails draftApprenticeshipDetails, CancellationToken cancellationToken)
        {
            ValidateStartDate(draftApprenticeshipDetails);
            ValidateUln(draftApprenticeshipDetails);
            await ValidateOverlaps(draftApprenticeshipDetails, cancellationToken);
            await ValidateReservation(providerId, accountId, accountLegalEntityPublicHashedId, draftApprenticeshipDetails, cancellationToken);
        }

        private void ValidateUln(DraftApprenticeshipDetails draftApprenticeshipDetails)
        {
            if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln)) return;

            switch (_ulnValidator.Validate(draftApprenticeshipDetails.Uln))
            {
                case UlnValidationResult.IsInValidTenDigitUlnNumber:
                    throw new DomainException(nameof(draftApprenticeshipDetails.Uln), "You must enter a 10-digit unique learner number");
                case UlnValidationResult.IsInvalidUln:
                    throw new DomainException(nameof(draftApprenticeshipDetails.Uln), "You must enter a valid unique learner number");
            }
        }

        private void ValidateStartDate(DraftApprenticeshipDetails details)
        {
            if (!details.StartDate.HasValue) return;

            if (details.StartDate.Value > _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
            {
                throw new DomainException(nameof(details.StartDate),
                    "The start date must be no later than one year after the end of the current teaching year");
            }
        }

        private async Task ValidateReservation(long providerId, long accountId, string accountLegalEntityPublicHashedId, DraftApprenticeshipDetails details, CancellationToken cancellationToken)
        {
            if (!details.ReservationId.HasValue) return;

            var validationRequest = new ReservationValidationRequest(providerId, accountId,
                accountLegalEntityPublicHashedId, details.ReservationId.Value, details.StartDate, details.TrainingProgramme?.CourseCode);

            var validationResult = await _reservationValidationService.Validate(validationRequest, cancellationToken);

            var errors = validationResult.ValidationErrors.Select(error => new DomainError(error.PropertyName, error.Reason)).ToList();
            if (errors.Any())
            {
                throw new DomainException(errors);
            }
        }

        private async Task ValidateOverlaps(DraftApprenticeshipDetails details, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(details.Uln) || !details.StartDate.HasValue || !details.EndDate.HasValue) return;

            var overlapResult = await _overlapCheckService.CheckForOverlaps(details.Uln, details.StartDate.Value.To(details.EndDate.Value), default, cancellationToken);

            if (!overlapResult.HasOverlaps) return;

            var errorMessage = "The date overlaps with existing dates for the same apprentice."
                               + Environment.NewLine +
                               "Please check the date - contact the employer for help";

            var errors = new List<DomainError>();

            if (overlapResult.HasOverlappingStartDate)
            {
                errors.Add(new DomainError(nameof(details.StartDate), errorMessage));
            }

            if (overlapResult.HasOverlappingEndDate)
            {
                errors.Add(new DomainError(nameof(details.EndDate), errorMessage));
            }

            throw new DomainException(errors);
        }
    }
}
