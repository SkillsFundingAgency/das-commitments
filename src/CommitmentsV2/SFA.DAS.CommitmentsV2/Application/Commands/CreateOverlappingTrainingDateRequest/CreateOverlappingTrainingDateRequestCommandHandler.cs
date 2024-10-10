using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest;

public class CreateOverlappingTrainingDateRequestCommandHandler(
    IOverlappingTrainingDateRequestDomainService overlappingTrainingDateRequestDomainService,
    IAuthenticationService authenticationService)
    : IRequestHandler<
        CreateOverlappingTrainingDateRequestCommand, CreateOverlappingTrainingDateResult>
{
    public async Task<CreateOverlappingTrainingDateResult> Handle(
        CreateOverlappingTrainingDateRequestCommand request, CancellationToken cancellationToken)
    {
        var originatingParty = authenticationService.GetUserParty();
        
        var result =
            await overlappingTrainingDateRequestDomainService.CreateOverlappingTrainingDateRequest(
                request.DraftApprenticeshipId,
                originatingParty,
                null,
                request.UserInfo,
                cancellationToken
            );

        return new CreateOverlappingTrainingDateResult
        {
            Id = result.Id
        };
    }
}