using HttpResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses;
using CommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice;

namespace SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers
{
    public class GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapper : IMapper<CommandResponse.GetDraftApprenticeResponse, HttpResponse.GetDraftApprenticeshipResponse>
    {
        public HttpResponse.GetDraftApprenticeshipResponse Map(CommandResponse.GetDraftApprenticeResponse source)
        {
            var draft = source.DraftApprenticeshipDetails;

            return new HttpResponse.GetDraftApprenticeshipResponse
            {
                Id = draft.Id,
                FirstName = draft.FirstName,
                LastName = draft.LastName,
                DateOfBirth = draft.DateOfBirth,
                Uln = draft.Uln,
                CourseCode = draft.TrainingProgramme?.CourseCode,
                Cost = draft.Cost,
                StartDate = draft.StartDate,
                EndDate = draft.EndDate,
                Reference = draft.Reference,
                ReservationId = draft.ReservationId
            };
        }
    }
}