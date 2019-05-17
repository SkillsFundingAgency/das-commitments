using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper : IMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>
    {
        public AddDraftApprenticeshipCommand Map(AddDraftApprenticeshipRequest source)
        {
            return new AddDraftApprenticeshipCommand(
                source.CohortId,
                source.UserId,
                source.ProviderId,
                source.CourseCode,
                source.Cost,
                source.StartDate,
                source.EndDate,
                source.OriginatorReference,
                source.ReservationId,
                source.FirstName,
                source.LastName,
                source.DateOfBirth,
                source.Uln);
        }
    }
}