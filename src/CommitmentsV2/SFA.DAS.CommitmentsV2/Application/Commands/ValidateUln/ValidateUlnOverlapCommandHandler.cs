using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateUln
{
    public class ValidateUlnOverlapCommandHandler : IRequestHandler<ValidateUlnOverlapCommand, ValidateUlnOverlapResult>
    {
        private readonly IOverlapCheckService _overlapCheckService;

        public ValidateUlnOverlapCommandHandler(IOverlapCheckService overlapCheckService)
        {
            _overlapCheckService = overlapCheckService;
        }

        public async Task<ValidateUlnOverlapResult> Handle(ValidateUlnOverlapCommand command, CancellationToken cancellationToken)
        {
            var result = await _overlapCheckService.CheckForOverlaps(command.ULN, command.StartDate.To(command.EndDate), command.ApprenticeshipId, cancellationToken);
            return new ValidateUlnOverlapResult
            {
                ULN = command.ULN,
                HasOverlappingStartDate = result.HasOverlappingEndDate,
                HasOverlappingEndDate = result.HasOverlappingEndDate
            };
        }
    }
}
