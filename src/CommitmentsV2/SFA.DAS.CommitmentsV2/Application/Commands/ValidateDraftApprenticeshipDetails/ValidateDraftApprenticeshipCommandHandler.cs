using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails
{
    public class ValidateDraftApprenticeshipCommandHandler : IRequestHandler<ValidateDraftApprenticeshipDetailsCommand>
    {
        private readonly IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly ICohortDomainService _cohortDomainService;

        public ValidateDraftApprenticeshipCommandHandler(
           IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
           ICohortDomainService cohortDomainService)
        {
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
        }

        public async Task Handle(ValidateDraftApprenticeshipDetailsCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(request);
            await _cohortDomainService.ValidateDraftApprenticeshipForOverlappingTrainingDateRequest(request.DraftApprenticeshipRequest.ProviderId, request.DraftApprenticeshipRequest.CohortId, draftApprenticeshipDetails, cancellationToken);
        }
    }
}
