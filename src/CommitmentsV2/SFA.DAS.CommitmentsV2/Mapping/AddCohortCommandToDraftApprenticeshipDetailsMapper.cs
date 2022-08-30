using System;
using System.Threading.Tasks;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class AddCohortCommandToDraftApprenticeshipDetailsMapper : IOldMapper<AddCohortCommand, DraftApprenticeshipDetails>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public AddCohortCommandToDraftApprenticeshipDetailsMapper(IAuthorizationService authorizationService, ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _authorizationService = authorizationService;
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }   

        public async Task<DraftApprenticeshipDetails> Map(AddCohortCommand source)
        {
            var trainingProgramme = await GetCourse(source.CourseCode, source.StartDate);
            
            var result = new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Uln = source.Uln,
                TrainingProgramme = trainingProgramme,
                DeliveryModel = source.DeliveryModel,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.OriginatorReference,
                ReservationId = source.ReservationId,
                EmploymentEndDate = source.EmploymentEndDate,
                EmploymentPrice = source.EmploymentPrice,
                IgnoreStartDateOverlap = source.IgnoreStartDateOverlap
            };

            // Only populate standard version specific items if start is specified.
            // The course is returned as latest version if no start date is specified
            // Which is fine for setting the training programmer.
            if (source.StartDate.HasValue)
            {
                result.TrainingCourseVersion = trainingProgramme?.Version;
                result.TrainingCourseVersionConfirmed = trainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard;
                result.StandardUId = trainingProgramme?.StandardUId;
            }

            return result;
        }

        private Task<TrainingProgramme> GetCourse(string courseCode, DateTime? startDate)
        {
            if (startDate.HasValue && int.TryParse(courseCode, out _))
            {
                return _trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(courseCode, startDate.Value);
            }

            return _trainingProgrammeLookup.GetTrainingProgramme(courseCode);
        }
    }
}
