using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship
{
    public class AddDraftApprenticeshipCommandHandler : IRequestHandler<AddDraftApprenticeshipCommand, AddDraftApprenticeshipResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<AddDraftApprenticeshipCommandHandler> _logger;
        private readonly IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly ICohortDomainService _cohortDomainService;

        public AddDraftApprenticeshipCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<AddDraftApprenticeshipCommandHandler> logger,
            IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
        }

        public async Task<AddDraftApprenticeshipResult> Handle(AddDraftApprenticeshipCommand request, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;
            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(request);
            var draftApprenticeship = await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, request.CohortId, draftApprenticeshipDetails, request.UserInfo, cancellationToken);
            
            await db.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation($"Added draft apprenticeship. Reservation-Id:{request.ReservationId} Commitment-Id:{request.CohortId} Apprenticeship-Id:{draftApprenticeship.Id}");
            
            var response = new AddDraftApprenticeshipResult
            {
                Id = draftApprenticeship.Id
            };

            return response;
        }
    }
}