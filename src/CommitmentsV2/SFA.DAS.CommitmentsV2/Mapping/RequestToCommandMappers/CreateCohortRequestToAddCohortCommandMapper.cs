using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

namespace SFA.DAS.CommitmentsV2.Mapping.RequestToCommandMappers
{
    public class CreateCohortRequestToAddCohortCommandMapper : IMapper<CreateCohortRequest, AddCohortCommand>
    {
        public Task<AddCohortCommand> Map(CreateCohortRequest source)
        {
            return Task.FromResult(new AddCohortCommand
            {
                AccountLegalEntityId = source.AccountLegalEntityId,
                ProviderId = source.ProviderId,
                Cost = source.Cost,
                CourseCode = source.CourseCode,
                EndDate = source.EndDate,
                OriginatorReference = source.OriginatorReference,
                ReservationId = source.ReservationId,
                StartDate = source.StartDate,
                DateOfBirth = source.DateOfBirth,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.Uln,
                UserInfo = source.UserInfo
            });
        }
    }
}