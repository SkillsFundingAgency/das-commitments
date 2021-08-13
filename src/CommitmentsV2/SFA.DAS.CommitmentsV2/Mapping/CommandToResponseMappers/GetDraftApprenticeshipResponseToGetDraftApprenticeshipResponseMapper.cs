using System.Threading.Tasks;
using HttpResponse = SFA.DAS.CommitmentsV2.Api.Types.Responses;
using CommandResponse = SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

namespace SFA.DAS.CommitmentsV2.Mapping.CommandToResponseMappers
{
    public class GetDraftApprenticeshipResponseToGetDraftApprenticeshipResponseMapper : IOldMapper<CommandResponse.GetDraftApprenticeshipQueryResult, HttpResponse.GetDraftApprenticeshipResponse>
    {
        public Task<HttpResponse.GetDraftApprenticeshipResponse> Map(CommandResponse.GetDraftApprenticeshipQueryResult source)
        {
            return Task.FromResult(new HttpResponse.GetDraftApprenticeshipResponse
            {
                Id = source.Id,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                DateOfBirth = source.DateOfBirth,
                Uln = source.Uln,
                CourseCode = source.CourseCode,
                TrainingCourseVersion = source.TrainingCourseVersion,
                TrainingCourseName = source.TrainingCourseName,
                TrainingCourseOption = source.TrainingCourseOption,
                StandardUId = source.StandardUId,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Reference = source.Reference,
                ReservationId = source.ReservationId,
                IsContinuation = source.IsContinuation,
                OriginalStartDate = source.OriginalStartDate,
                HasStandardOptions = source.HasStandardOptions
            });
        }
    }
}