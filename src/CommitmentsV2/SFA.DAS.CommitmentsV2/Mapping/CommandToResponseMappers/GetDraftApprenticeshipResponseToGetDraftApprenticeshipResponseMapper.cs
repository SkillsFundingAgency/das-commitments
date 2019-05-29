using System.Threading.Tasks;
using HttpResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses;
using CommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice;

namespace SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers
{
    public class GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapper : IMapper<CommandResponse.GetDraftApprenticeResponse, HttpResponse.GetDraftApprenticeshipResponse>
    {
        public Task<HttpResponse.GetDraftApprenticeshipResponse> Map(CommandResponse.GetDraftApprenticeResponse source)
        {
            return Task.FromResult(new HttpResponse.GetDraftApprenticeshipResponse
            {
                Id = source.Id,
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
            });
        }
    }
}