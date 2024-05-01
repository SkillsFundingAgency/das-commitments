using System;
using System.Threading.Tasks;
using SFA.DAS.Authorization.Services;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateDraftApprenticeshipDetails;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping
{
    public class ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper : IMapper<ValidateDraftApprenticeshipDetailsCommand, DraftApprenticeshipDetails>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public ValidateDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(IAuthorizationService authorizationService, ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _authorizationService = authorizationService;
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task<DraftApprenticeshipDetails> Map(ValidateDraftApprenticeshipDetailsCommand comand)
        {
            var source = comand.DraftApprenticeshipRequest;

            var startDate = source.IsOnFlexiPaymentPilot.GetValueOrDefault() ? source.ActualStartDate : source.StartDate;
            var trainingProgrammeTask = GetCourse(source.CourseCode, startDate);
            var trainingProgramme = await trainingProgrammeTask;

            var result = new DraftApprenticeshipDetails
            {
                Id = source.Id,
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
                ActualStartDate = source.ActualStartDate,
                IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot
            };

            // Only populate standard version specific items if start is specified.
            // The course is returned as latest version if no start date is specified
            // Which is fine for setting the training programmer.
            if (startDate.HasValue)
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