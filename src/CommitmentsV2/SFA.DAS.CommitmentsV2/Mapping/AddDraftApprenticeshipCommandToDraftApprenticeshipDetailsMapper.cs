using System;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper : IMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>
    {
        private readonly ITrainingProgrammeApiClient _trainingProgrammeApiClient;
        private readonly IMapper<ITrainingProgramme, TrainingProgramme> _trainingProgrammeMapper;
        
        public AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(
            ITrainingProgrammeApiClient trainingProgrammeApiClient,
            IMapper<ITrainingProgramme, TrainingProgramme> trainingProgrammeMapper)
        {
            _trainingProgrammeApiClient = trainingProgrammeApiClient;
            _trainingProgrammeMapper = trainingProgrammeMapper;
        }

        public async Task<DraftApprenticeshipDetails> Map(AddDraftApprenticeshipCommand source)
        {
            var trainingProgram = await GetCourse(source.CourseCode);

            var result = new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.Uln,
                TrainingProgramme = trainingProgram,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.OriginatorReference,
                ReservationId = source.ReservationId,
            };

            return result;
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

            return await _trainingProgrammeMapper.Map(course);
        }
    }
}