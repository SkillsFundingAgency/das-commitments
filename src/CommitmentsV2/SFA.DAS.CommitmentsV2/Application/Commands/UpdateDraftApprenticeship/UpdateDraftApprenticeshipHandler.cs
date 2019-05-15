using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship
{
    public class UpdateDraftApprenticeshipHandler : IRequestHandler<UpdateDraftApprenticeshipCommand, UpdateDraftApprenticeshipResponse>
    {
        public Task<UpdateDraftApprenticeshipResponse> Handle(UpdateDraftApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            var response = new UpdateDraftApprenticeshipResponse();

            return Task.FromResult(response);
        }
    }
}
