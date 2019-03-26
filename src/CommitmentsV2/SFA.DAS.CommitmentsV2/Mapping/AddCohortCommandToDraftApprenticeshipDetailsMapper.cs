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

        public AddCohortCommandToDraftApprenticeshipDetailsMapper(ITrainingProgrammeApiClient trainingProgrammeApiClient)
        {
            _trainingProgrammeApiClient = trainingProgrammeApiClient;
        }

        public async Task<DraftApprenticeshipDetails> Map(AddCohortCommand source)
        {
            var trainingProgram = await GetCourseName(source.CourseCode);

            return new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Uln = source.ULN,
                TrainingType = (int?)(trainingProgram?.ProgrammeType), //todo: why is this not an enum?
                TrainingCode = source.CourseCode,
                TrainingName = trainingProgram?.ExtendedTitle,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                ProviderRef = source.OriginatorReference //todo: this won't work then the employer creates
            };
        }
        private async Task<ITrainingProgramme> GetCourseName(string courseCode)
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

            return course;
        }
    }
}
