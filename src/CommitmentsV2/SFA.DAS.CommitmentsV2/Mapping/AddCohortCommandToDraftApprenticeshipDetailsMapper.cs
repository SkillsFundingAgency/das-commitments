using System;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;


namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class AddCohortCommandToDraftApprenticeshipDetailsMapper : IMapper<AddCohortCommand, DraftApprenticeshipDetails>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public AddCohortCommandToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task<DraftApprenticeshipDetails> Map(AddCohortCommand source)
        {
            var trainingProgram = await GetCourse(source.CourseCode);

            var result = new DraftApprenticeshipDetails
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
                ReservationId = source.ReservationId,
            };

            return result;
        }

        private Task<TrainingProgramme> GetCourse(string courseCode)
        {
            return _trainingProgrammeLookup.GetTrainingProgramme(courseCode);
        }
    }
}
