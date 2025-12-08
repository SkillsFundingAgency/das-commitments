using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.Email;
using SFA.DAS.CommitmentsV2.Application.Commands.Reference;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

public class DraftApprenticeshipSetReferenceRequestToCommandMapper : IOldMapper<DraftApprenticeshipSetReferenceRequest, DraftApprenticeshipSetReferenceCommand>
{
    public Task<DraftApprenticeshipSetReferenceCommand> Map(DraftApprenticeshipSetReferenceRequest source)
    {
        return Task.FromResult(new DraftApprenticeshipSetReferenceCommand
        {
            CohortId = source.CohortId,
            Reference = source.Reference,
             Party = source.Party
        });
    }
}
