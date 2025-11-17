using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.EmailValidationService;

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
    }

    private static IEnumerable<DomainError> BuildCostValidationFailures(DraftApprenticeshipDetails draft)
    {
        if (IsAnOldLearnerRecordOrIsManualEntry())
        {
            if (draft.Cost.HasValue && draft.Cost <= 0)
            {
                yield return new DomainError(nameof(draft.Cost),
                    "Enter the total agreed training cost");
                yield break;
            }

            if (draft.Cost.HasValue &&
                draft.Cost > Constants.MaximumApprenticeshipCost)
            {
                yield return new DomainError(nameof(draft.Cost),
                    "The total cost must be £100,000 or less");
            }
        }
        else
        {
            if (draft.Cost.GetValueOrDefault() <= 0)
            {
                yield return new DomainError(nameof(draft.Cost),
                    "Total agreed apprenticeship price cannot be £0 - re-submit your ILR file with correct training price (TNP1) and end-point assessment price (TNP2)");
                yield break;
            }

            if (draft.TrainingPrice == 0 && draft.Cost > 0 || draft.TrainingPrice < 0)
            {
                yield return new DomainError(nameof(draft.TrainingPrice),
                    "Training price (TNP1) must be in the range of 1-100000 - re-submit your ILR file with correct training price");
            }

            if (draft.EndPointAssessmentPrice < 0)
            {
                yield return new DomainError(nameof(draft.EndPointAssessmentPrice),
                    "End-point assessment price (TNP2) should be in the range of 1-100000 - re-submit your ILR file with correct end-point assessment price");
            }

            if (draft.Cost.HasValue &&
                draft.Cost > Constants.MaximumApprenticeshipCost)
            {
                yield return new DomainError(nameof(draft.Cost),
                    "Total agreed apprenticeship price must be 100000 or less - re-submit your ILR file with correct training price (TNP1) and end-point assessment price (TNP2)");
            }

            if (draft.Cost.GetValueOrDefault() != draft.TrainingPrice.GetValueOrDefault() +
                draft.EndPointAssessmentPrice.GetValueOrDefault())
            {
                yield return new DomainError(nameof(draft.Cost),
                    "The agreed apprenticeship price must equal training price (TNP1) + end-point assessment price (TNP2) - re-submit your ILR file with correct data");
            }
        }
        yield break;

        bool IsAnOldLearnerRecordOrIsManualEntry() => draft.LearnerDataId == null ||
                                                      (draft.TrainingPrice == null &&
                                                       draft.EndPointAssessmentPrice == null);
    }

    private static IEnumerable<DomainError> BuildDateOfBirthValidationFailures(DraftApprenticeshipDetails draftApprenticeshipDetails, int minimumAgeAtApprenticeshipStart, int maximumAgeAtApprenticeshipStart)
    {
        if (draftApprenticeshipDetails.AgeOnStartDate.HasValue && draftApprenticeshipDetails.AgeOnStartDate.Value < minimumAgeAtApprenticeshipStart)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The apprentice must be at least {minimumAgeAtApprenticeshipStart} years old at the start of their training");
            yield break;
        }

        if (draftApprenticeshipDetails.AgeOnStartDate.HasValue && draftApprenticeshipDetails.AgeOnStartDate.Value >= maximumAgeAtApprenticeshipStart)
        {
            yield return new DomainError(nameof(draftApprenticeshipDetails.DateOfBirth), $"The apprentice must be {maximumAgeAtApprenticeshipStart-1} years or under at the start of their training");
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