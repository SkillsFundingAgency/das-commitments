using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateUln;

public class ValidateUlnOverlapCommandHandler(IOverlapCheckService overlapCheckService) : IRequestHandler<ValidateUlnOverlapCommand, ValidateUlnOverlapResult>
{
    public async Task<ValidateUlnOverlapResult> Handle(ValidateUlnOverlapCommand command, CancellationToken cancellationToken)
    {
        var result = await overlapCheckService.CheckForOverlaps(command.ULN, command.StartDate.To(command.EndDate), command.ApprenticeshipId, cancellationToken);
        
        return new ValidateUlnOverlapResult
        {
            ULN = command.ULN,
            HasOverlappingStartDate = result.HasOverlappingStartDate,
            HasOverlappingEndDate = result.HasOverlappingEndDate
        };
    }
}