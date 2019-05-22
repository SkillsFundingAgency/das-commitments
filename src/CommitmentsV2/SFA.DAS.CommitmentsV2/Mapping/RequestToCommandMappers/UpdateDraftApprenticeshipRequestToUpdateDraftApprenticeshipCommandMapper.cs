using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapper : IMapper<UpdateDraftApprenticeshipRequest, UpdateDraftApprenticeshipCommand>
    {
        public UpdateDraftApprenticeshipCommand Map(UpdateDraftApprenticeshipRequest source)
        {
            return new UpdateDraftApprenticeshipCommand
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                DateOfBirth = source.DateOfBirth,
                Uln = source.Uln,
                CourseCode = source.CourseCode,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Reference = source.Reference,
                ReservationId = source.ReservationId
            };
        }
    }
}