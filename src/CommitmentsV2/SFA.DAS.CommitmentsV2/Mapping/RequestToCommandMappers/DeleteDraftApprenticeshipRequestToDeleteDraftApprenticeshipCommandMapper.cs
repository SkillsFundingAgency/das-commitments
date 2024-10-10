using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers;

public class DeleteDraftApprenticeshipRequestToDeleteDraftApprenticeshipCommandMapper : IOldMapper<DeleteDraftApprenticeshipRequest, DeleteDraftApprenticeshipCommand>
{
    public Task<DeleteDraftApprenticeshipCommand> Map(DeleteDraftApprenticeshipRequest source)
    {
        return Task.FromResult(new DeleteDraftApprenticeshipCommand
        {
            UserInfo = source.UserInfo
        });
    }
}