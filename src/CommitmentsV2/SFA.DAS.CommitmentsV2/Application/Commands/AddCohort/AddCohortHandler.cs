using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.QueryExtensions;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.HashingService;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortHandler : IRequestHandler<AddCohortCommand, AddCohortResponse>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IHashingService _hashingService;
        private readonly ILogger<AddCohortHandler> _logger;

        private readonly IAsyncMapper<AddCohortCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;

        public AddCohortHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext, 
            IHashingService hashingService, 
            ILogger<AddCohortHandler> logger,
            IAsyncMapper<AddCohortCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper)
        {
            _dbContext = dbContext;
            _hashingService = hashingService;
            _logger = logger;
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
        }

        public async Task<AddCohortResponse> Handle(AddCohortCommand command, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            var commitment = await SaveCohortWithLogging(db, command, cancellationToken);

            var response = new AddCohortResponse
            {
                Id = commitment.Id,
                Reference = commitment.Reference
            };

            return response;
        }

        private async Task<Commitment> SaveCohortWithLogging(ProviderCommitmentsDbContext db, AddCohortCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var result = await AddCohort(db, command, cancellationToken);
                _logger.LogInformation($"Saved-commitment Provider: {command.ProviderId} Account-Legal-Entity:{command.AccountLegalEntityId} Reservation-Id:{command.ReservationId} Commitment-Id:{result?.Id} Apprenticeship:{result?.Apprenticeship?.First()?.Id}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Saving-commitment provider: {command.ProviderId} account-legal-entity:{command.AccountLegalEntityId} reservation-id:{command.ReservationId}");
                throw;
            }
        }

        private async Task<Commitment> AddCohort(ProviderCommitmentsDbContext db, AddCohortCommand command, CancellationToken cancellationToken)
        {
            var commitment = await AddCommitment(db, command, cancellationToken);

            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(command);

            commitment.AddDraftApprenticeship(draftApprenticeshipDetails);           

            await db.SaveChangesAsync(cancellationToken);

            commitment.Reference = _hashingService.HashValue(commitment.Id);

            await db.SaveChangesAsync(cancellationToken);

            return commitment;
        }

        private Task<AccountLegalEntityDetailsNeededForCohort> GetLegalEntityDetails(ProviderCommitmentsDbContext db,
            long accountLegalEntityId, CancellationToken cancellationToken)
        {
            return db.AccountLegalEntities.GetById(accountLegalEntityId,
                ale => new AccountLegalEntityDetailsNeededForCohort
                {
                    Name = ale.Name,
                    Address = ale.Address,
                    PublicHashedId = ale.PublicHashedId,
                    Id = ale.Id,
                    AccountId = ale.AccountId,
                    OrganisationType = ale.OrganisationType,
                    OrganisationId = ale.LegalEntityId
                }, cancellationToken);
        }

        private Task<ProviderDetailsNeededForCohort> GetProviderDetails(ProviderCommitmentsDbContext db, long ukPrn,
            CancellationToken cancellationToken)
        {
            return db.Providers.GetById(ukPrn, 
                ale => new ProviderDetailsNeededForCohort{UkPrn = ukPrn, Name = ale.Name}, 
                cancellationToken);
        }

        private async Task<Commitment> AddCommitment(ProviderCommitmentsDbContext db, AddCohortCommand command, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var accountLegalEntity = await GetAccountLegalEntity(db, command, cancellationToken);

            var providerDetails = await GetProvider(db, command, cancellationToken);

            var commitment = new Commitment
            {
                // Reference cannot be set until we've saved the commitment (as we need the Id) but it's non-nullable so we'll use a temp value
                Reference = "",
                EmployerAccountId = accountLegalEntity.AccountId,
                LegalEntityId = accountLegalEntity.OrganisationId,
                LegalEntityName = accountLegalEntity.Name,
                LegalEntityAddress = accountLegalEntity.Address,
                LegalEntityOrganisationType = accountLegalEntity.OrganisationType,
                ProviderId = providerDetails.UkPrn,
                ProviderName = providerDetails.Name,
                CommitmentStatus = CommitmentStatus.New,
                EditStatus = EditStatus.ProviderOnly,
                CreatedOn = now,
                LastAction = LastAction.None,
                AccountLegalEntityPublicHashedId = accountLegalEntity.PublicHashedId,
                Originator = Originator.Provider
            };

            db.Commitment.Add(commitment);

            return commitment;
        }

        private async Task<ProviderDetailsNeededForCohort> GetProvider(ProviderCommitmentsDbContext db, AddCohortCommand command,
            CancellationToken cancellationToken)
        {
            var providerDetails = await GetProviderDetails(db, command.ProviderId, cancellationToken);
            if (providerDetails == null)
            {
                throw new BadRequestException($"The provider with id {command.ProviderId} does not exist");
            }

            return providerDetails;
        }

        private async Task<AccountLegalEntityDetailsNeededForCohort> GetAccountLegalEntity(ProviderCommitmentsDbContext db, AddCohortCommand command,
            CancellationToken cancellationToken)
        {
            var accountLegalEntity = await GetLegalEntityDetails(db, command.AccountLegalEntityId, cancellationToken);

            if (accountLegalEntity == null)
            {
                throw new BadRequestException(
                    $"The account legal entity with id {command.AccountLegalEntityId} does not exist");
            }

            return accountLegalEntity;
        }

        private class AccountLegalEntityDetailsNeededForCohort
        {
            public long Id { get; set; }
            public long AccountId { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string OrganisationId { get; set; }
            public OrganisationType OrganisationType { get; set; }
            public string PublicHashedId { get; set; }
        }

        private class ProviderDetailsNeededForCohort
        {
            public long UkPrn { get; set; }
            public string Name { get; set; }
        }
    }
}
