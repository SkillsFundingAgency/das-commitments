using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class ValidateDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper : IOldMapper<AddDraftApprenticeshipRequest, ValidateDraftApprenticeshipCommand>
    {
        public Task<ValidateDraftApprenticeshipCommand> Map(AddDraftApprenticeshipRequest source)
        {
            return new DraftApprenticeshipCommandBaseMapper().Map<ValidateDraftApprenticeshipCommand>(source);
        }
    }
}