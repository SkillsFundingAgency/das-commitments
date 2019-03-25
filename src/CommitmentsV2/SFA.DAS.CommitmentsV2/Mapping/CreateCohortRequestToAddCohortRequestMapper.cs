using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Helper;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class CreateCohortRequestToAddCohortRequestMapper : IMapper<CreateCohortRequest, AddCohortCommand>
    {
        public AddCohortCommand Map(CreateCohortRequest source)
        {
            return new AddCohortCommand
            {
                AccountLegalEntityId = source.AccountLegalEntityId,
                ProviderId = source.ProviderId,
                Cost = source.Cost,
                CourseCode = source.CourseCode,
                EndDate = DateHelper.ConvertToNullableDate(source.CourseEndMonth, source.CourseEndYear),
                OriginatorReference = source.OriginatorReference,
                ReservationId = source.ReservationId,
                StartDate = DateHelper.ConvertToNullableDate(source.CourseStartMonth, source.CourseStartYear),
                UserId = source.UserId,
                DateOfBirth = DateHelper.ConvertToNullableDate(source.BirthDay, source.BirthMonth, source.BirthYear),
                FirstName = source.FirstName,
                LastName = source.LastName,
                ULN = source.Uln
            };
        }
    }
}