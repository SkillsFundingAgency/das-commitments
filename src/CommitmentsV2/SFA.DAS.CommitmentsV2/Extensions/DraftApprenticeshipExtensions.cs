﻿using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmailValidationService;
using SFA.DAS.CommitmentsV2.Shared.Extensions;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DraftApprenticeshipExtensions
{
    public static List<DomainError> ValidateDraftApprenticeshipDetails(this DraftApprenticeshipDetails draftApprenticeshipDetails, bool isContinuation, long? transferSenderId, ICollection<ApprenticeshipBase> apprenticeships, int minimumAgeAtApprenticeshipStart, int maximumAgeAtApprenticeshipStart)
    {
        var errors = new List<DomainError>();
        errors.AddRange(BuildEndDateValidationFailures(draftApprenticeshipDetails));
        errors.AddRange(BuildCostValidationFailures(draftApprenticeshipDetails));
        errors.AddRange(BuildFlexibleEmploymentValidationFailures(draftApprenticeshipDetails));
        errors.AddRange(BuildFirstNameValidationFailures(draftApprenticeshipDetails));
        errors.AddRange(BuildLastNameValidationFailures(draftApprenticeshipDetails));
        errors.AddRange(BuildEmailValidationFailures(draftApprenticeshipDetails));
        errors.AddRange(BuildDateOfBirthValidationFailures(draftApprenticeshipDetails, minimumAgeAtApprenticeshipStart, maximumAgeAtApprenticeshipStart));
        if (!isContinuation)
        {
            errors.AddRange(BuildIsOnFlexiPaymentPilotValidationFailures(draftApprenticeshipDetails));
            errors.AddRange(BuildStartDateValidationFailures(draftApprenticeshipDetails, transferSenderId));
            errors.AddRange(BuildUlnValidationFailures(draftApprenticeshipDetails, apprenticeships));
            errors.AddRange(BuildTrainingProgramValidationFailures(draftApprenticeshipDetails, transferSenderId));
        }
        return errors;
    }

    private static IEnumerable<DomainError> BuildFirstNameValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.FirstName))
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.FirstName), "First name must be entered");
        }
    }

    private static IEnumerable<DomainError> BuildLastNameValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.LastName))
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.LastName), "Last name must be entered");
        }
    }

    private static IEnumerable<DomainError> BuildEmailValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails.Email != null && !draftApprenticeshipDetails.Email.IsAValidEmailAddress())
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.Email), "Please enter a valid email address");
        }
    }

    private static IEnumerable<DomainError> BuildIsOnFlexiPaymentPilotValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (!draftApprenticeshipDetails.IsOnFlexiPaymentPilot.HasValue)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.IsOnFlexiPaymentPilot), "Select whether this apprentice will be on the pilot programme");
        }
    }

    private static IEnumerable<DomainError> BuildEndDateValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails.EndDate.HasValue && draftApprenticeshipDetails.EndDate < Constants.DasStartDate)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The end date must not be earlier than May 2017");
            yield break;
        }

        if (draftApprenticeshipDetails.EndDate.HasValue && draftApprenticeshipDetails.StartDate.HasValue && draftApprenticeshipDetails.EndDate <= draftApprenticeshipDetails.StartDate)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The end date must not be on or before the start date");
        }

        if ((draftApprenticeshipDetails.IsOnFlexiPaymentPilot ?? true) && draftApprenticeshipDetails.EndDate.HasValue && draftApprenticeshipDetails.ActualStartDate.HasValue && draftApprenticeshipDetails.EndDate <= draftApprenticeshipDetails.ActualStartDate)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The end date must not be on or before the start date");
        }
        else if ((draftApprenticeshipDetails.IsOnFlexiPaymentPilot ?? true) && draftApprenticeshipDetails.ActualStartDate.HasValue && draftApprenticeshipDetails.EndDate.HasValue)
        {
            var differenceBetweenStartAndEnd = draftApprenticeshipDetails.EndDate.Value - draftApprenticeshipDetails.ActualStartDate.Value;
            differenceBetweenStartAndEnd = differenceBetweenStartAndEnd.Add(new TimeSpan(1, 0, 0, 0));
            if (differenceBetweenStartAndEnd.Days < 365)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The duration of an apprenticeship must be at least 365 days");
            }

            var maxEndDate = draftApprenticeshipDetails.ActualStartDate.Value.AddYears(10).AddDays(-1);
            if (draftApprenticeshipDetails.EndDate.Value > maxEndDate)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.EndDate), "The projected finish date should be no more than ten years in the future");
            }
        }
    }

    private static IEnumerable<DomainError> BuildCostValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails.IsOnFlexiPaymentPilot.GetValueOrDefault())
        {
            if (draftApprenticeshipDetails.TrainingPrice.GetValueOrDefault() + draftApprenticeshipDetails.EndPointAssessmentPrice.GetValueOrDefault() > Constants.MaximumApprenticeshipCost)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "Total price for training and end-point assessment cannot be more than £100,000");
                yield return new DomainError(nameof(draftApprenticeshipDetails.TrainingPrice), " ");
                yield return new DomainError(nameof(draftApprenticeshipDetails.EndPointAssessmentPrice), " ");
                yield break;
            }

            if (draftApprenticeshipDetails.TrainingPrice.HasValue && draftApprenticeshipDetails.TrainingPrice <= 0)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.TrainingPrice), "The Training Price must be in the range of 1-100000");
            }

            if (draftApprenticeshipDetails.EndPointAssessmentPrice.HasValue && draftApprenticeshipDetails.EndPointAssessmentPrice <= 0)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.EndPointAssessmentPrice), "The End-Point Assessment Price must be in the range of 1-100000");
            }
        }
        else
        {
            if (draftApprenticeshipDetails.Cost.HasValue && draftApprenticeshipDetails.Cost <= 0)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "Enter the total agreed training cost");
                yield break;
            }

            if (draftApprenticeshipDetails.Cost.HasValue && draftApprenticeshipDetails.Cost > Constants.MaximumApprenticeshipCost)
            {
                yield return new DomainError(nameof(draftApprenticeshipDetails.Cost), "The total cost must be £100,000 or less");
            }
        }
    }

    private static IEnumerable<DomainError> BuildDateOfBirthValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails, int minimumAgeAtApprenticeshipStart, int maximumAgeAtApprenticeshipStart)
    {
        if (!draftApprenticeshipDetails.IsOnFlexiPaymentPilot.GetValueOrDefault() && draftApprenticeshipDetails.AgeOnStartDate.HasValue && draftApprenticeshipDetails.AgeOnStartDate.Value < minimumAgeAtApprenticeshipStart)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The apprentice must be at least {minimumAgeAtApprenticeshipStart} years old at the start of their training");
            yield break;
        }

        if (draftApprenticeshipDetails.AgeOnStartDate.HasValue && draftApprenticeshipDetails.AgeOnStartDate.Value >= maximumAgeAtApprenticeshipStart)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The apprentice must be {maximumAgeAtApprenticeshipStart} years old or younger at the start of their training");
            yield break;
        }

        if (draftApprenticeshipDetails.DateOfBirth.HasValue && draftApprenticeshipDetails.DateOfBirth < Constants.MinimumDateOfBirth)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The Date of birth is not valid");
        }
    }

    private static IEnumerable<DomainError> BuildTrainingProgramValidationFailures(DraftApprenticeshipDetails details, long? transferSenderId)
    {
        if (details.DeliveryModel == null)
        {
            yield return new DomainError(nameof(details.DeliveryModel), "You must select the apprenticeship delivery model");
        }

        if (details.TrainingProgramme == null) yield break;

        if (details.TrainingProgramme?.ProgrammeType == ProgrammeType.Framework && transferSenderId.HasValue)
        {
            yield return new DomainError(nameof(details.TrainingProgramme.CourseCode), "Entered course is not valid.");
        }
    }

    private static IEnumerable<DomainError> BuildStartDateValidationFailures(DraftApprenticeshipDetails details, long? transferSenderId)
    {
        if (!details.StartDate.HasValue && !details.ActualStartDate.HasValue) yield break;
        var startDate = details.StartDate.HasValue ? details.StartDate.Value : details.ActualStartDate.Value;                
        var startDateField = details.StartDate.HasValue ? nameof(details.StartDate) : nameof(details.ActualStartDate);

        var courseStartedBeforeDas = details.TrainingProgramme != null &&
                                     (!details.TrainingProgramme.EffectiveFrom.HasValue ||
                                      details.TrainingProgramme.EffectiveFrom.Value < Constants.DasStartDate);

        var trainingProgrammeStatus = details.TrainingProgramme?.GetStatusOn(startDate);

        if ((startDate < Constants.DasStartDate) && (!trainingProgrammeStatus.HasValue || courseStartedBeforeDas))
        {
            yield return new DomainError(startDateField, "The start date must not be earlier than May 2017");
            yield break;
        }

        if (trainingProgrammeStatus.HasValue && trainingProgrammeStatus.Value != TrainingProgrammeStatus.Active)
        {
            var suffix = trainingProgrammeStatus == TrainingProgrammeStatus.Pending
                ? $"after {details.TrainingProgramme.EffectiveFrom.Value.AddMonths(-1):MM yyyy}"
                : $"before {details.TrainingProgramme.EffectiveTo.Value.AddMonths(1):MM yyyy}";

            var errorMessage = $"This training course is only available to apprentices with a start date {suffix}";

            yield return new DomainError(startDateField, errorMessage);
            yield break;
        }

        if (trainingProgrammeStatus.HasValue && transferSenderId.HasValue
                                             && startDate < Constants.TransferFeatureStartDate)
        {
            var errorMessage = $"Apprentices funded through a transfer can't start earlier than May 2018";

            yield return new DomainError(startDateField, errorMessage);
            yield break;
        }

        if (details.IsOnFlexiPaymentPilot.GetValueOrDefault() &&
            details.DateOfBirth.HasValue &&
            details.ActualStartDate.HasValue &&
            details.ActualStartDate.Value <= details.DateOfBirth.Value.GetLastFridayInJuneOfSchoolYearApprenticeTurned16())
        {
            yield return new DomainError(startDateField, $"The start date must be after {details.DateOfBirth.Value.GetLastFridayInJuneOfSchoolYearApprenticeTurned16().ToGdsFormat()}, when the learner has reached school leaving age");
        }

        if (details.IsOnFlexiPaymentPilot.GetValueOrDefault() &&      
            details.ActualStartDate.HasValue &&
            details.ActualStartDate.Value < Constants.SimplifiedPaymentsStartDate)
        {
            yield return new DomainError(startDateField, $"The start date must not be earlier than {Constants.SimplifiedPaymentsStartDate:d MMMM yyyy}.");
        }
    }

    private static IEnumerable<DomainError> BuildUlnValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails, ICollection<ApprenticeshipBase> apprenticeships)
    {
        if (string.IsNullOrWhiteSpace(draftApprenticeshipDetails.Uln))
        {
            yield break;
        }

        if (apprenticeships != null && apprenticeships.Any(a => a.Id != draftApprenticeshipDetails.Id && a.Uln == draftApprenticeshipDetails.Uln))
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.Uln), "The unique learner number has already been used for an apprentice in this cohort");
        }
    }

    private static IEnumerable<DomainError> BuildFlexibleEmploymentValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails.DeliveryModel != DeliveryModel.PortableFlexiJob)
        {
            yield break;
        }

        foreach (var failure in BuildFlexibleEmploymentPriceValidationFailures(draftApprenticeshipDetails))
        {
            yield return failure;
        }

        foreach (var failure in BuildFlexibleEmploymentDateValidationFailures(draftApprenticeshipDetails))
        {
            yield return failure;
        }
    }

    private static IEnumerable<DomainError> BuildFlexibleEmploymentDateValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails.EmploymentEndDate == null)
        {
            yield break;
        }

        if (draftApprenticeshipDetails.EmploymentEndDate.Value < draftApprenticeshipDetails.StartDate?.AddMonths(3))
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EmploymentEndDate), "This date must be at least 3 months later than the planned apprenticeship training start date");
        }

        if (draftApprenticeshipDetails.EmploymentEndDate.Value > draftApprenticeshipDetails.EndDate)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EmploymentEndDate), "This date must not be later than the projected apprenticeship training end date");
        }
    }

    private static IEnumerable<DomainError> BuildFlexibleEmploymentPriceValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails)
    {
        if (draftApprenticeshipDetails.EmploymentPrice == null)
        {
            yield break;
        }

        if (draftApprenticeshipDetails.EmploymentPrice <= 0)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EmploymentPrice), "The cost must be greater than zero");
        }

        if (draftApprenticeshipDetails.EmploymentPrice > Constants.MaximumApprenticeshipCost)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EmploymentPrice), "The agreed price for this employment must be £100,000 or less");
        }

        if (draftApprenticeshipDetails.Cost.GetValueOrDefault() <= 0)
        {
            yield break;
        }

        if (draftApprenticeshipDetails.EmploymentPrice > draftApprenticeshipDetails.Cost)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.EmploymentPrice), "This price must not be more than the total agreed apprenticeship price");
        }
    }
}