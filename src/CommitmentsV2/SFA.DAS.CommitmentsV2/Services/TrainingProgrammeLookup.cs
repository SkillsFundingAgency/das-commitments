using System;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class TrainingProgrammeLookup : ITrainingProgrammeLookup
    {
        private readonly ITrainingProgrammeApiClient _trainingProgrammeApiClient;
        private readonly IMapper<ITrainingProgramme, TrainingProgramme> _trainingProgrammeMapper;

        public TrainingProgrammeLookup(
            ITrainingProgrammeApiClient trainingProgrammeApiClient,
            IMapper<ITrainingProgramme, TrainingProgramme> trainingProgrammeMapper)
        {
            _trainingProgrammeApiClient = trainingProgrammeApiClient;
            _trainingProgrammeMapper = trainingProgrammeMapper;
        }

        public async Task<TrainingProgramme> GetTrainingProgramme(string courseCode)
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