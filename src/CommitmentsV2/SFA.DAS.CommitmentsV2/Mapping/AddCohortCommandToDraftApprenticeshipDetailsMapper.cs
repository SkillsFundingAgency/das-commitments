using System;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class AddCohortCommandToDraftApprenticeshipDetailsMapper : IAsyncMapper<AddCohortCommand, DraftApprenticeshipDetails>
    {
        private readonly ITrainingProgrammeApiClient _trainingProgrammeApiClient;
        private readonly IMapper<ITrainingProgramme, TrainingProgramme> _trainingProgrammeMapper;

        public AddCohortCommandToDraftApprenticeshipDetailsMapper(
            ITrainingProgrammeApiClient trainingProgrammeApiClient,
            IMapper<ITrainingProgramme, TrainingProgramme> trainingProgrammeMapper)
        {
            _trainingProgrammeApiClient = trainingProgrammeApiClient;
            _trainingProgrammeMapper = trainingProgrammeMapper;
        }

        public async Task<DraftApprenticeshipDetails> Map(AddCohortCommand source)
        {
            var trainingProgram = await GetCourse(source.CourseCode);

            return new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.ULN,
                TrainingProgramme = trainingProgram,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.OriginatorReference,
                ReservationId = source.ReservationId
            };
        }
        private async Task<TrainingProgramme> GetCourse(string courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return null;
            }

            var course = await _trainingProgrammeApiClient.GetTrainingProgramme(courseCode);

            if (course == null)
            {
                throw new Exception($"The course code {courseCode} was not found");
            }

            return _trainingProgrammeMapper.Map(course);
        }
    }
}
