using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload
{
    public class BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper : IMapper<BulkUploadAddDraftApprenticeshipRequest, DraftApprenticeshipDetails>
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task<DraftApprenticeshipDetails> Map(BulkUploadAddDraftApprenticeshipRequest source)
        {
            var trainingProgrammeTask = GetCourse(source.CourseCode, source.StartDate);
            var trainingProgramme = await trainingProgrammeTask;

            var result = new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Uln = source.Uln,
                TrainingProgramme = trainingProgramme,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.OriginatorReference,
                ReservationId = source.ReservationId
            };

            result.TrainingCourseVersion = trainingProgramme?.Version;
            result.TrainingCourseVersionConfirmed = trainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard;
            result.StandardUId = trainingProgramme?.StandardUId;
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
