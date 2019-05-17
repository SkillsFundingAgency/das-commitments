using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship
{
    public class AddDraftApprenticeshipCommandHandler : AsyncRequestHandler<AddDraftApprenticeshipCommand>
    {
        private readonly IAsyncMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
        private readonly ICohortDomainService _cohortDomainService;

        public AddDraftApprenticeshipCommandHandler(
            IAsyncMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
            ICohortDomainService cohortDomainService)
        {
            _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
            _cohortDomainService = cohortDomainService;
        }

        protected override async Task Handle(AddDraftApprenticeshipCommand request, CancellationToken cancellationToken)
        {
            var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(request);
            
            await _cohortDomainService.AddDraftApprenticeship(request.ProviderId, request.CohortId, draftApprenticeshipDetails, cancellationToken);
        }
    }
}