using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeship
{
    public class ValidateDraftApprenticeshipCommandHandler : IRequestHandler<ValidateDraftApprenticeshipCommand, ValidateDraftApprenticeshipResult>
    {
        private readonly IOldMapper<DraftApprenticeshipCommandBase, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly ICohortDomainService _cohortDomainService;

        public ValidateDraftApprenticeshipCommandHandler(
            IOldMapper<DraftApprenticeshipCommandBase, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService)
        {
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
        }

        public async Task<ValidateDraftApprenticeshipResult> Handle(ValidateDraftApprenticeshipCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(request);
            var draftApprenticeship = await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, request.CohortId, draftApprenticeshipDetails, request.UserInfo, cancellationToken);

            var response = new ValidateDraftApprenticeshipResult();

            return response;
        }
    }
}