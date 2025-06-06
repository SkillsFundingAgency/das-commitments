﻿using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions;

public static class EditApprenticeshipExtension
{
    public static EditApprenticeshipValidationRequest CreateValidationRequest(this EditApprenticeshipCommand request, Apprenticeship apprenticeship, DateTime dateTimeNow)
    {
        static string GetValue(string sourceString, string apprenticeshipString)
        {
            if (!string.IsNullOrWhiteSpace(sourceString))
                return sourceString;

            return apprenticeshipString;
        }

        var source = request.EditApprenticeshipRequest;
        var validationRequest = new EditApprenticeshipValidationRequest
        {
            ApprenticeshipId = apprenticeship.Id,
            EmployerAccountId = apprenticeship.Cohort.EmployerAccountId,
            ProviderId = apprenticeship.Cohort.ProviderId,
            DeliveryModel = source.DeliveryModel ?? apprenticeship.DeliveryModel ?? DeliveryModel.Regular,
            EmploymentEndDate = source.EmploymentEndDate ?? apprenticeship.FlexibleEmployment?.EmploymentEndDate,
            EmploymentPrice = source.EmploymentPrice ?? apprenticeship.FlexibleEmployment?.EmploymentPrice,
            CourseCode = GetValue(source.CourseCode, apprenticeship.CourseCode),
            Version = GetValue(source.Version, apprenticeship.TrainingCourseVersion),
            Option = GetValue(source.Option, apprenticeship.TrainingCourseOption),
            FirstName = GetValue(source.FirstName, apprenticeship.FirstName),
            LastName = GetValue(source.LastName, apprenticeship.LastName),
            Email = GetValue(source.Email, apprenticeship.Email),
            EmployerReference = source.EmployerReference,
            ULN = GetValue(source.ULN, apprenticeship.Uln),
            DateOfBirth = source.DateOfBirth ?? apprenticeship.DateOfBirth,
            EndDate = source.EndDate ?? apprenticeship.EndDate,
            StartDate = source.StartDate ?? apprenticeship.StartDate,
            ActualStartDate = source.ActualStartDate ?? apprenticeship.ActualStartDate,
            Cost = source.Cost ?? apprenticeship.PriceHistory.GetPrice(dateTimeNow),
            TrainingPrice = source.TrainingPrice ?? apprenticeship.PriceHistory.GetTrainingPrice(dateTimeNow),
            EndPointAssessmentPrice = source.EndPointAssessmentPrice ?? apprenticeship.PriceHistory.GetAssessmentPrice(dateTimeNow),
            ProviderReference = source.ProviderReference
        };

        return validationRequest;
    }

    public static ApprenticeshipUpdate MapToApprenticeshipUpdate(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party, DateTime utcDateTime)
    {
        var apprenticeshipUpdate = new ApprenticeshipUpdate();
        apprenticeshipUpdate.DeliveryModel = command.EditApprenticeshipRequest.DeliveryModel;
        apprenticeshipUpdate.EmploymentEndDate = command.EditApprenticeshipRequest.EmploymentEndDate;
        apprenticeshipUpdate.EmploymentPrice = command.EditApprenticeshipRequest.EmploymentPrice;
        apprenticeshipUpdate.TrainingCode = command.EditApprenticeshipRequest.CourseCode;
        apprenticeshipUpdate.TrainingCourseVersion = command.EditApprenticeshipRequest.Version;
        apprenticeshipUpdate.TrainingCourseOption = command.EditApprenticeshipRequest.Option;
        apprenticeshipUpdate.DateOfBirth = command.EditApprenticeshipRequest.DateOfBirth;
        apprenticeshipUpdate.EndDate = command.EditApprenticeshipRequest.EndDate;
        apprenticeshipUpdate.StartDate = command.EditApprenticeshipRequest.StartDate;
        apprenticeshipUpdate.ActualStartDate = command.EditApprenticeshipRequest.ActualStartDate;
        apprenticeshipUpdate.FirstName = command.EditApprenticeshipRequest.FirstName;
        apprenticeshipUpdate.LastName = command.EditApprenticeshipRequest.LastName;
        apprenticeshipUpdate.Email = command.EditApprenticeshipRequest.Email;
        apprenticeshipUpdate.Cost = command.EditApprenticeshipRequest.Cost;
        apprenticeshipUpdate.TrainingPrice = command.EditApprenticeshipRequest.TrainingPrice;
        apprenticeshipUpdate.EndPointAssessmentPrice = command.EditApprenticeshipRequest.EndPointAssessmentPrice;
        apprenticeshipUpdate.ApprenticeshipId = apprenticeship.Id;
        apprenticeshipUpdate.Originator = party == Party.Employer ? Originator.Employer : Originator.Provider;
        apprenticeshipUpdate.UpdateOrigin = ApprenticeshipUpdateOrigin.ChangeOfCircumstances;
        apprenticeshipUpdate.EffectiveFromDate = apprenticeship.StartDate;
        apprenticeshipUpdate.CreatedOn = utcDateTime;

        return apprenticeshipUpdate;
    }

    public static bool IntermediateApprenticeshipUpdateRequired(this EditApprenticeshipApiRequest request, Apprenticeship apprenticeship)
    {
        return !string.IsNullOrWhiteSpace(request.FirstName)
               || !string.IsNullOrWhiteSpace(request.LastName)
               || !string.IsNullOrWhiteSpace(request.Email)
               || request.DeliveryModel != null
               || request.EmploymentEndDate != null
               || request.EmploymentPrice != null
               || !string.IsNullOrWhiteSpace(request.CourseCode)
               || !string.IsNullOrWhiteSpace(request.Version)
               || request.Option != apprenticeship.TrainingCourseOption
               || request.DateOfBirth != null
               || request.StartDate != null
               || request.ActualStartDate != null
               || request.EndDate != null
               || request.Cost != null;
    }

    public static bool EmployerReferenceUpdateRequired(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
    {
        return apprenticeship.EmployerRef != command.EditApprenticeshipRequest.EmployerReference && party == Party.Employer;
    }

    public static bool ProviderReferenceUpdateRequired(this EditApprenticeshipCommand command, Apprenticeship apprenticeship, Party party)
    {
        return apprenticeship.ProviderRef != command.EditApprenticeshipRequest.ProviderReference && party == Party.Provider;
    }
}