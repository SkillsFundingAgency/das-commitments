using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping.BulkUpload;

public class BulkUploadAddDraftApprenticeshipRequestToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup) 
    : IMapper<BulkUploadAddDraftApprenticeshipsCommand, List<DraftApprenticeshipDetails>>
{
    public async Task<List<DraftApprenticeshipDetails>> Map(BulkUploadAddDraftApprenticeshipsCommand command)
    {
        var draftApprenticeshipDetailsList = new List<DraftApprenticeshipDetails>();
        foreach (var source in command.BulkUploadDraftApprenticeships)
        {
            var result = new DraftApprenticeshipDetails
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Email = source.Email,
                Uln = source.Uln,
                Cost = source.Cost,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                DateOfBirth = source.DateOfBirth,
                Reference = source.ProviderRef,
                ReservationId = source.ReservationId,
                DeliveryModel = Types.DeliveryModel.Regular,
                RecognisePriorLearning = source.RecognisePriorLearning,
                TrainingTotalHours = source.TrainingTotalHours,
                DurationReducedByHours = source.TrainingHoursReduction,
                DurationReducedBy = source.DurationReducedBy,
                IsDurationReducedByRPL = source.IsDurationReducedByRPL,
                PriceReducedBy = source.PriceReducedBy
            };
            await MapTrainingProgramme(source, result);
            
            draftApprenticeshipDetailsList.Add(result);
        }

        return draftApprenticeshipDetailsList;
    }

    private async Task MapTrainingProgramme(BulkUploadAddDraftApprenticeshipRequest source, DraftApprenticeshipDetails result)
    {
        var trainingProgrammeTask = GetCourse(source.CourseCode, source.StartDate);
        var trainingProgramme = await trainingProgrammeTask;
        result.TrainingProgramme = trainingProgramme;
        result.TrainingCourseVersion = trainingProgramme?.Version;
        result.TrainingCourseVersionConfirmed = trainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard;
        result.StandardUId = trainingProgramme?.StandardUId;
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