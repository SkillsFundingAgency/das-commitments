using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;

public class ValidateDraftApprenticeshipCommandHandler(
    IMapper<ValidateDraftApprenticeshipDetailsCommand,
        DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<ValidateDraftApprenticeshipDetailsCommand>
{
    public async Task Handle(ValidateDraftApprenticeshipDetailsCommand request, CancellationToken cancellationToken)
    {
        var draftApprenticeshipDetails = await draftApprenticeshipDetailsMapper.Map(request);

        await cohortDomainService.ValidateDraftApprenticeshipForOverlappingTrainingDateRequest(
            request.DraftApprenticeshipRequest.ProviderId,
            request.DraftApprenticeshipRequest.CohortId,
            draftApprenticeshipDetails,
            cancellationToken
        );
    }
}