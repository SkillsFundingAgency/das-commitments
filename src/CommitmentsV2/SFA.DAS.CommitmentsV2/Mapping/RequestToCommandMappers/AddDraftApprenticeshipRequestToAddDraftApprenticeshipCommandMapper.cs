using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class AddDraftApprenticeshipRequestToAddDraftApprenticeshipCommandMapper : IOldMapper<AddDraftApprenticeshipRequest, AddDraftApprenticeshipCommand>
    {
        public Task<AddDraftApprenticeshipCommand> Map(AddDraftApprenticeshipRequest source)
        {
            return Task.FromResult(new AddDraftApprenticeshipCommand
            {
                UserId = source.UserId,
                ProviderId = source.ProviderId,
                CourseCode = source.CourseCode,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                OriginatorReference = source.OriginatorReference,
                ReservationId = source.ReservationId,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                DateOfBirth = source.DateOfBirth,
                Uln = source.Uln,
                UserInfo = source.UserInfo
            });
        }
    }
}