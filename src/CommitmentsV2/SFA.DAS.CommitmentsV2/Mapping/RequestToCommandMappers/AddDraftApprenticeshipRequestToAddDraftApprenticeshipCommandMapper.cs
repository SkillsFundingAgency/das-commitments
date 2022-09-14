using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper : IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>
    {
        public Task<AddDraftApprenticeshipCommand> Map(AddDraftApprenticeshipRequest source)
        {
            return new DraftApprenticeshipCommandBaseMapper().Map<AddDraftApprenticeshipCommand>(source);
        }
    }
}