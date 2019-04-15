using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.HashingService;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortHandler : IRequestHandler<AddCohortCommand, AddCohortResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IHashingService _hashingService;
        private readonly ILogger<AddCohortHandler> _logger;

        private readonly IAsyncMapper<AddCohortCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly IUlnValidator _ulnValidator;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;

        public AddCohortHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext, 
            IHashingService hashingService, 
            ILogger<AddCohortHandler> logger,
            IAsyncMapper<AddCohortCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
            IUlnValidator ulnValidator, ICurrentDateTime currentDateTime, IAcademicYearDateProvider academicYearDateProvider)
        {
            _dbContext = dbContext;
            _hashingService = hashingService;
            _logger = logger;
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _ulnValidator = ulnValidator;
            _currentDateTime = currentDateTime;
            _academicYearDateProvider = academicYearDateProvider;
        }

        public async Task<AddCohortResponse> Handle(AddCohortCommand command, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var provider = await db.Providers.SingleOrDefaultAsync(p => p.UkPrn == command.ProviderId, cancellationToken);
            if (provider == null) throw new BadRequestException($"Provider {command.ProviderId} was not found");
            var accountLegalEntity = await db.AccountLegalEntities.SingleOrDefaultAsync(x => x.Id == command.AccountLegalEntityId, cancellationToken);
            if (accountLegalEntity == null) throw new BadRequestException($"AccountLegalEntity {command.AccountLegalEntityId} was not found");

            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(command);

            var cohort = provider.CreateCohort(accountLegalEntity, draftApprenticeshipDetails, _ulnValidator, _currentDateTime, _academicYearDateProvider);

            db.Commitment.Add(cohort);
            await db.SaveChangesAsync(cancellationToken);

            cohort.Reference = _hashingService.HashValue(cohort.Id);

            await db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Saved cohort. Provider: {command.ProviderId} Account-Legal-Entity:{command.AccountLegalEntityId} Reservation-Id:{command.ReservationId} Commitment-Id:{cohort?.Id} Apprenticeship:{cohort?.Apprenticeship?.First()?.Id}");

            var response = new AddCohortResponse
            {
                Id = cohort.Id,
                Reference = cohort.Reference
            };

            return response;
        }
    }
}
