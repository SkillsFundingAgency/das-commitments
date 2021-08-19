using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;


namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper : IOldMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task<DraftApprenticeshipDetails> Map(UpdateDraftApprenticeshipCommand source)
        {
            var trainingProgram = await GetCourse(source.CourseCode, source.StartDate);
            return new DraftApprenticeshipDetails
            {
                Id = source.ApprenticeshipId,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Uln = source.Uln,
                TrainingProgramme = trainingProgram,
                StandardUId = trainingProgram?.StandardUId,
                TrainingCourseVersion = trainingProgram?.Version,
                TrainingCourseVersionConfirmed = trainingProgram != null,
                TrainingCourseOption = source.CourseOption,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.Reference,
                ReservationId = source.ReservationId
            };
        }

        private Task<TrainingProgramme> GetCourse(string courseCode, DateTime? startDate)
        {
            return startDate.HasValue ? _trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(courseCode, startDate.Value) : _trainingProgrammeLookup.GetTrainingProgramme(courseCode);
        }
    }
}
