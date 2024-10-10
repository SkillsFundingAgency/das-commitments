using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;

public class CreateChangeOfPartyRequestCommandHandler(IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService)
    : IRequestHandler<CreateChangeOfPartyRequestCommand>
{
    public Task Handle(CreateChangeOfPartyRequestCommand command, CancellationToken cancellationToken)
    {
        return changeOfPartyRequestDomainService.CreateChangeOfPartyRequest(
            command.ApprenticeshipId,
            command.ChangeOfPartyRequestType,
            command.NewPartyId,
            command.NewPrice,
            command.NewStartDate,
            command.NewEndDate,
            command.UserInfo,
            command.NewEmploymentPrice,
            command.NewEmploymentEndDate,
            command.DeliveryModel,
            command.HasOverlappingTrainingDates,
            cancellationToken
        );
    }
}