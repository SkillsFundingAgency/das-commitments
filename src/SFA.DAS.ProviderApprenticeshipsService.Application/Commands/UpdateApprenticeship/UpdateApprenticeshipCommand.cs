using MediatR;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.UpdateApprenticeship
{
    public class UpdateApprenticeshipCommand : IAsyncRequest
    {
        public long ProviderId { get; set; }
        public Apprenticeship Apprenticeship { get; set; }
    }
}