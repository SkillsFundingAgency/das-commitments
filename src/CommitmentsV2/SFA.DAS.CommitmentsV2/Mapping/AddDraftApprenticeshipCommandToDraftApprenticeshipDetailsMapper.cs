using System.Threading.Tasks;
using SFA.DAS.Authorization;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper : IMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(IAuthorizationService authorizationService, ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _authorizationService = authorizationService;
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task<DraftApprenticeshipDetails> Map(AddDraftApprenticeshipCommand source)
        {
            var trainingProgrammeTask = GetCourse(source.CourseCode);
            var trainingProgramme = await trainingProgrammeTask;
            
            var result = new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.Uln,
                TrainingProgramme = trainingProgramme,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.OriginatorReference,
                ReservationId = source.ReservationId
            };

            return result;
        }

        private Task<TrainingProgramme> GetCourse(string courseCode)
        {
            return _trainingProgrammeLookup.GetTrainingProgramme(courseCode);
        }
    }
}