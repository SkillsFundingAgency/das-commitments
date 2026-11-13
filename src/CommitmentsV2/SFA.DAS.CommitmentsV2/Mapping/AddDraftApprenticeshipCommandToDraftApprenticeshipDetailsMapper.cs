using SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;

namespace SFA.DAS.CommitmentsV2.Mapping;

public class AddDraftApprenticeshipCommandToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup) 
    : IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails>
{
    public async Task<DraftApprenticeshipDetails> Map(AddDraftApprenticeshipCommand source)
    {
        var startDate = source.StartDate;
        var trainingProgrammeTask = GetCourse(source.CourseCode, startDate);
        var trainingProgramme = await trainingProgrammeTask;

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
            ActualStartDate = source.ActualStartDate,
            EndDate = source.EndDate,
            DateOfBirth = source.DateOfBirth,
            Reference = source.OriginatorReference,
            ReservationId = source.ReservationId,
            EmploymentEndDate = source.EmploymentEndDate,
            EmploymentPrice = source.EmploymentPrice,
            IgnoreStartDateOverlap = source.IgnoreStartDateOverlap,
            TrainingPrice = source.TrainingPrice,
            EndPointAssessmentPrice = source.EndPointAssessmentPrice,
            LearnerDataId = source.LearnerDataId
        };

        return result.ApplyTrainingCourseVersion(source.TrainingCourseVersion, source.StandardUId,
            populateStandardVersion: startDate.HasValue);
    }

    private Task<TrainingProgramme> GetCourse(string courseCode, DateTime? startDate)
    {
        if (startDate.HasValue && int.TryParse(courseCode, out _))
        {
            return trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(courseCode, startDate.Value);
        }

        return trainingProgrammeLookup.GetTrainingProgramme(courseCode);
    }
}