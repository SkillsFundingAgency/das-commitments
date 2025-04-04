﻿using SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Mapping;

public class UpdateDraftApprenticeshipToDraftApprenticeshipDetailsMapper(ITrainingProgrammeLookup trainingProgrammeLookup) : IOldMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails>
{
    public async Task<DraftApprenticeshipDetails> Map(UpdateDraftApprenticeshipCommand source)
    {
        var startDate = source.IsOnFlexiPaymentPilot.GetValueOrDefault() ? source.ActualStartDate : source.StartDate;
        var trainingProgramme = await GetCourse(source.CourseCode, startDate);
        var result = new DraftApprenticeshipDetails
        {
            Id = source.ApprenticeshipId,
            FirstName = source.FirstName,
            LastName = source.LastName,
            Email = source.Email,
            Uln = source.Uln,
            TrainingProgramme = trainingProgramme,
            TrainingCourseOption = source.CourseOption,
            DeliveryModel = source.DeliveryModel,
            EmploymentPrice = source.EmploymentPrice,
            Cost = source.Cost,
            TrainingPrice = source.TrainingPrice,
            EndPointAssessmentPrice = source.EndPointAssessmentPrice,
            StartDate = source.StartDate,
            ActualStartDate = source.ActualStartDate,
            EmploymentEndDate = source.EmploymentEndDate,
            EndDate = source.EndDate,
            DateOfBirth = source.DateOfBirth,
            Reference = source.Reference,
            ReservationId = source.ReservationId,
            IgnoreStartDateOverlap = source.IgnoreStartDateOverlap,
            IsOnFlexiPaymentPilot = source.IsOnFlexiPaymentPilot
        };

        // Only populate standard version specific items if start is specified.
        // The course is returned as latest version if no start date is specified
        // Which is fine for setting the training programmer.
        if (!source.IsContinuation  && startDate.HasValue)
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
            return trainingProgrammeLookup.GetCalculatedTrainingProgrammeVersion(courseCode, startDate.Value);
        }

        return trainingProgrammeLookup.GetTrainingProgramme(courseCode);
    }
}