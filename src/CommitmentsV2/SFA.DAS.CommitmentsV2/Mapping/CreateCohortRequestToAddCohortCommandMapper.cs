using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class CreateCohortRequestToAddCohortCommandMapper : IMapper<CreateCohortRequest, AddCohortCommand>
    {
        public AddCohortCommand Map(CreateCohortRequest source)
        {
            return new AddCohortCommand
            {
                AccountLegalEntityId = source.AccountLegalEntityId,
                ProviderId = source.ProviderId,
                Cost = source.Cost,
                CourseCode = source.CourseCode,
                EndDate = source.EndDate,
                OriginatorReference = source.OriginatorReference,
                ReservationId = source.ReservationId,
                StartDate = source.StartDate,
                UserId = source.UserId,
                DateOfBirth = source.DateOfBirth,
                FirstName = source.FirstName,
                LastName = source.LastName,
                ULN = source.Uln
            };
        }
    }
}