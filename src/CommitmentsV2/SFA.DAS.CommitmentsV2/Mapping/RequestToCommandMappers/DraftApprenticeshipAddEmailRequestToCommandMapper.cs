using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.Email;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

public class DraftApprenticeshipAddEmailRequestToCommandMapper : IOldMapper<DraftApprenticeshipAddEmailRequest, DraftApprenticeshipAddEmailCommand>
{
    public Task<DraftApprenticeshipAddEmailCommand> Map(DraftApprenticeshipAddEmailRequest source)
    {
        return Task.FromResult(new DraftApprenticeshipAddEmailCommand
        {
            CohortId = source.CohortId,
            Email = source.Email,
            EndDate = source.EndDate,
            StartDate = source.StartDate,
        });
    }
}
