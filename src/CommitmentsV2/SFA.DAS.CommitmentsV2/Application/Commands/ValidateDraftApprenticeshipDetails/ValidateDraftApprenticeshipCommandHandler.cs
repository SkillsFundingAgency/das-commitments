using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails
{
    public class ValidateDraftApprenticeshipCommandHandler : IRequestHandler<ValidateDraftApprenticeshipDetailsCommand>
    {
        private readonly ILogger<ValidateDraftApprenticeshipCommandHandler> _logger;
        private readonly IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly ICohortDomainService _cohortDomainService;

        public ValidateDraftApprenticeshipCommandHandler(
           ILogger<ValidateDraftApprenticeshipCommandHandler> logger,
           IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
           ICohortDomainService cohortDomainService)
        {
             _logger = logger;
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
        }

        public async Task<Unit> Handle(ValidateDraftApprenticeshipDetailsCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(request);
            await _cohortDomainService.ValidateDraftApprenticeshipForOverlappingTrainingDateRequest(request.DraftApprenticeshipRequest.ProviderId, request.DraftApprenticeshipRequest.CohortId, draftApprenticeshipDetails, cancellationToken);
            return Unit.Value;
        }
    }
}
