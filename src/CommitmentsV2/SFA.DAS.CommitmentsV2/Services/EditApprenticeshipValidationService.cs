using MediatR;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgramme;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Types;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.EmailValidationService;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class EditApprenticeshipValidationService : IEditApprenticeshipValidationService
    {
        private readonly IProviderCommitmentsDbContext _context;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly IReservationValidationService _reservationValidationService;
        private readonly IAcademicYearDateProvider _academicYearDateProvider;
        private readonly IMediator _mediator;
        private readonly ICurrentDateTime _currentDateTime;
        private readonly IAuthenticationService _authenticationService;

        public EditApprenticeshipValidationService(IProviderCommitmentsDbContext context,
            IMediator mediator,
            IOverlapCheckService overlapCheckService,
            IReservationValidationService reservationValidationService,
            IAcademicYearDateProvider academicYearDateProvider,
            ICurrentDateTime currentDateTime,
            IAuthenticationService authenticationService)
        {
            _context = context;
            _overlapCheckService = overlapCheckService;
            _reservationValidationService = reservationValidationService;
            _academicYearDateProvider = academicYearDateProvider;
            _mediator = mediator;
            _currentDateTime = currentDateTime;
            _authenticationService = authenticationService;
        }

        public async Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request, CancellationToken cancellationToken)
        {
            var errors = new List<DomainError>();
            var apprenticeship = _context.Apprenticeships
                .Include(y => y.Cohort)
                .Include(y => y.PriceHistory)
                .Include(y => y.FlexibleEmployment)
                .FirstOrDefault(x => x.Id == request.ApprenticeshipId);

            if (apprenticeship == null)
            {
                return null;
            }

            errors.AddRange(NoChangeValidationFailures(request, apprenticeship));
            if (errors.Count == 0)
            {
                CheckForInvalidOperations(request, apprenticeship);
                errors.AddRange(BuildFirstNameValidationFailures(request, apprenticeship));
                errors.AddRange(BuildLastNameValidationFailures(request, apprenticeship));
                errors.AddRange(BuildDateOfBirthValidationFailures(request, apprenticeship));
                errors.AddRange(BuildStartDateValidationFailures(request, apprenticeship));
                errors.AddRange(BuildEndDateValidationFailures(request, apprenticeship));
                errors.AddRange(BuildCostValidationFailures(request, apprenticeship));
                errors.AddRange(BuildEmployerRefValidationFailures(request, apprenticeship));
                errors.AddRange(BuildProviderRefValidationFailures(request, apprenticeship));
                errors.AddRange(BuildOverlapValidationFailures(request, apprenticeship));
                errors.AddRange(await BuildReservationValidationFailures(request, apprenticeship));
                errors.AddRange(BuildTrainingProgramValidationFailures(request, apprenticeship));
                errors.AddRange(BuildEmailValidationFailures(request, apprenticeship));
                errors.AddRange(BuildFlexibleEmploymentValidationFailures(request, apprenticeship));
            }

            if (errors.Count == 0)
            {
                var overlapError = await EmailOverlapValidationFailures(request, apprenticeship);

                if (overlapError != null)
                {
                    errors.Add(overlapError);
                }
            }

            return new EditApprenticeshipValidationResult()
            {
                Errors = errors
            };
        }

        private void CheckForInvalidOperations(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeship)
        {
            var party = _authenticationService.GetUserParty();

            if (request.ULN != apprenticeship.Uln)
            {
                throw new InvalidOperationException("Invalid operation - ULN can't be modified.");
            }

            if (apprenticeship.IsContinuation)
            {
                if (request.FirstName != apprenticeship.FirstName
                    || request.LastName != apprenticeship.LastName
                    || request.DateOfBirth != apprenticeship.DateOfBirth)
                {
                    throw new InvalidOperationException("Invalid operation - First name, Last name,DOB can't change for continuation records.");
                }
            }

            if (IsLockedForUpdate(apprenticeship) || IsUpdateLockedForStartDateAndCourse(apprenticeship) || apprenticeship.IsContinuation)
            {
                if (request.CourseCode != apprenticeship.CourseCode)
                {
                    throw new InvalidOperationException("Invalid operation - training code can't change for the current state of the object.");
                }
                if (request.DeliveryModel != (apprenticeship.DeliveryModel ?? DeliveryModel.Regular))
                {
                    throw new InvalidOperationException("Invalid operation - delivery model can't change for the current state of the object.");
                }
            }

            if (IsLockedForUpdate(apprenticeship) || IsUpdateLockedForStartDateAndCourse(apprenticeship))
            {
                if (request.StartDate != apprenticeship.StartDate)
                {
                    throw new InvalidOperationException("Invalid operation - start date can't change for the current state of the object.");
                }
            }

            if (IsEndDateLocked(apprenticeship))
            {
                if (request.EndDate != apprenticeship.EndDate)
                {
                    throw new InvalidOperationException("Invalid operation - End date can't change for the current state of the object.");
                }
                if (request.EmploymentEndDate != apprenticeship.FlexibleEmployment?.EmploymentEndDate)
                {
                    throw new InvalidOperationException("Invalid operation - Employment End date can't change for the current state of the object.");
                }
            }

            if (IsLockedForUpdate(apprenticeship))
            {
                if (request.Cost != apprenticeship.PriceHistory.GetPrice(_currentDateTime.UtcNow))
                {
                    throw new InvalidOperationException("Invalid operation - Cost can't change for the current state of the object.");
                }
                if (request.EmploymentPrice != apprenticeship.FlexibleEmployment?.EmploymentPrice)
                {
                    throw new InvalidOperationException("Invalid operation - Employment Price can't change for the current state of the object.");
                }
            }
        }

        private IEnumerable<DomainError> BuildTrainingProgramValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {

            if (request.DeliveryModel == null)
            {
                yield return new DomainError(nameof(request.DeliveryModel), "Delivery model is required");
            }

            if (!string.IsNullOrEmpty(request.CourseCode))
            {
                if (request.CourseCode != apprenticeshipDetails.CourseCode)
                {
                    var result = _mediator.Send(new GetTrainingProgrammeQuery
                    {
                        Id = request.CourseCode
                    }).Result;

                    if (result.TrainingProgramme.ProgrammeType == ProgrammeType.Framework && apprenticeshipDetails.Cohort.TransferSenderId.HasValue)
                    {
                        yield return new DomainError(nameof(request.CourseCode), "Entered course is not valid.");
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(request.CourseCode), "Invalid training code");
            }

        }

        private IEnumerable<DomainError> NoChangeValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeship)
        {
            var referenceNotUpdated = _authenticationService.GetUserParty() == Party.Employer
                 ? request.EmployerReference == apprenticeship.EmployerRef
                 : request.ProviderReference == apprenticeship.ProviderRef;

            if (request.FirstName == apprenticeship.FirstName
                && request.LastName == apprenticeship.LastName
                && request.DateOfBirth == apprenticeship.DateOfBirth
                && request.Email == apprenticeship.Email
                && request.EndDate == apprenticeship.EndDate
                && request.Cost == apprenticeship.PriceHistory.GetPrice(_currentDateTime.UtcNow)
                && request.StartDate == apprenticeship.StartDate
                && request.DeliveryModel == apprenticeship.DeliveryModel
                && request.CourseCode == apprenticeship.CourseCode
                && request.ULN == apprenticeship.Uln
                && request.Version == apprenticeship.TrainingCourseVersion
                && request.Option == apprenticeship.TrainingCourseOption
                && request.EmploymentEndDate == apprenticeship.FlexibleEmployment?.EmploymentEndDate
                && request.EmploymentPrice == apprenticeship.FlexibleEmployment?.EmploymentPrice
                && referenceNotUpdated)
            {
                yield return new DomainError("ApprenticeshipId", "No change made: you need to amend details or cancel");
            }
        }

        private IEnumerable<DomainError> BuildFirstNameValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                if (request.FirstName != apprenticeshipDetails.FirstName)
                {
                    if (request.FirstName.Length > 100)
                    {
                        yield return new DomainError(nameof(request.FirstName), "You must enter a first name that's no longer than 100 characters");
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(request.FirstName), "First name must be entered");
            }
        }

        private IEnumerable<DomainError> BuildLastNameValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                if (request.LastName != apprenticeshipDetails.LastName)
                {
                    if (request.LastName.Length > 100)
                    {
                        yield return new DomainError(nameof(request.LastName), "You must enter a last name that's no longer than 100 characters");
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(request.LastName), "Last name must be entered");
            }
        }

        private IEnumerable<DomainError> BuildEmailValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (apprenticeshipDetails.Email != null && string.IsNullOrWhiteSpace(request.Email))
            {
                yield return new DomainError(nameof(request.Email), "Email address cannot be blank");
            }

            if (apprenticeshipDetails.Email == null && !string.IsNullOrWhiteSpace(request.Email) && apprenticeshipDetails.Cohort.EmployerAndProviderApprovedOn < new DateTime(2021,09,10))
            {
                yield return new DomainError(nameof(request.Email), "Email update cannot be requested");
            }

            if (request.Email != apprenticeshipDetails.Email && !string.IsNullOrWhiteSpace(request.Email))
            {
                if (!request.Email.IsAValidEmailAddress())
                {
                    yield return new DomainError(nameof(request.Email), "Please enter a valid email address");
                }
            }
        }

        private async Task<DomainError> EmailOverlapValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            bool NoChangesRequested() => (request.Email == apprenticeshipDetails.Email && request.StartDate == apprenticeshipDetails.StartDate && request.EndDate == apprenticeshipDetails.EndDate);

            if (string.IsNullOrWhiteSpace(request.Email))
                return null;

            if (NoChangesRequested())
                return null;

            var startDate = request.StartDate.Value;
            var endDate = request.EndDate.Value;

            var range = startDate.To(endDate);

            var overlap = await _overlapCheckService.CheckForEmailOverlaps(request.Email, range, request.ApprenticeshipId, null, CancellationToken.None);

            if (overlap != null)
            {
                return new DomainError(nameof(request.Email), overlap.BuildErrorMessage());
            }

            return null;
        }

        private async Task<IEnumerable<DomainError>> BuildReservationValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeship)
        {
            List<DomainError> errors = new List<DomainError>();
            if (apprenticeship.ReservationId.HasValue && request.StartDate.HasValue && !string.IsNullOrWhiteSpace(request.CourseCode))
            {
                var validationRequest = new ReservationValidationRequest(apprenticeship.ReservationId.Value, request.StartDate.Value, request.CourseCode);
                var validationResult = await _reservationValidationService.Validate(validationRequest, CancellationToken.None);

                errors = validationResult.ValidationErrors.Select(error => new DomainError(error.PropertyName, error.Reason)).ToList();
            }

            return errors;
        }

        private IEnumerable<DomainError> BuildOverlapValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeship)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue)
            {
                var errorMessage = $"The date overlaps with existing training dates for the same apprentice. Please check the date - contact the {(_authenticationService.GetUserParty() == Party.Employer ? "training provider" : "employer")} for help";
                var overlapResult = _overlapCheckService.CheckForOverlaps(apprenticeship.Uln, request.StartDate.Value.To(request.EndDate.Value), apprenticeship.Id, CancellationToken.None).Result;

                if (overlapResult.HasOverlappingStartDate)
                {
                    yield return new DomainError(nameof(request.StartDate), errorMessage);
                }

                if (overlapResult.HasOverlappingEndDate)
                {
                    yield return new DomainError(nameof(request.EndDate), errorMessage);
                }
            }
        }

        private IEnumerable<DomainError> BuildCostValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (request.Cost.HasValue)
            {
                if (request.Cost != apprenticeshipDetails.PriceHistory.GetPrice(_currentDateTime.UtcNow))
                {
                    if (request.Cost <= 0)
                    {
                        yield return new DomainError(nameof(request.Cost), "Enter the total agreed training cost");
                        yield break;
                    }

                    if (request.Cost > Constants.MaximumApprenticeshipCost)
                    {
                        yield return new DomainError(nameof(request.Cost), "The total cost must be £100,000 or less");
                    }

                    if (request.Cost.Value - Math.Truncate(request.Cost.Value) > 0)
                    {
                        yield return new DomainError(nameof(request.Cost), "Enter the total agreed training cost");
                        yield break;
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(request.Cost), "Enter the total agreed training cost");
            }
        }

        private IEnumerable<DomainError> BuildEmployerRefValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (_authenticationService.GetUserParty() == Party.Employer && request.EmployerReference != apprenticeshipDetails.EmployerRef)
            {
                if (!string.IsNullOrWhiteSpace(request.EmployerReference) && request.EmployerReference.Length > 20)
                {
                    yield return new DomainError(nameof(request.EmployerReference), "The Reference must be 20 characters or fewer");
                }
            }
        }

        private IEnumerable<DomainError> BuildProviderRefValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (_authenticationService.GetUserParty() == Party.Provider && request.ProviderReference != apprenticeshipDetails.ProviderRef)
            {
                if (!string.IsNullOrWhiteSpace(request.ProviderReference) && request.ProviderReference.Length > 20)
                {
                    yield return new DomainError(nameof(request.ProviderReference), "The Reference must be 20 characters or fewer");
                }
            }
        }

        private IEnumerable<DomainError> BuildDateOfBirthValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (request.DateOfBirth.HasValue)
            {
                if (request.DateOfBirth.Value != apprenticeshipDetails.DateOfBirth.Value)
                {
                    if (request.DateOfBirth < Constants.MinimumDateOfBirth)
                    {
                        yield return new DomainError(nameof(apprenticeshipDetails.DateOfBirth), $"The Date of birth is not valid");
                        yield break;
                    }

                    if (request.StartDate.HasValue)
                    {
                        var ageOnStartDate = AgeOnStartDate(request.DateOfBirth, request.StartDate);
                        if (ageOnStartDate.HasValue && ageOnStartDate.Value < Constants.MinimumAgeAtApprenticeshipStart)
                        {
                            yield return new DomainError(nameof(apprenticeshipDetails.DateOfBirth), $"The apprentice must be at least {Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training");
                            yield break;
                        }

                        if (ageOnStartDate.HasValue && ageOnStartDate >= Constants.MaximumAgeAtApprenticeshipStart)
                        {
                            yield return new DomainError(nameof(apprenticeshipDetails.DateOfBirth), $"The apprentice must be younger than {Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training");
                            yield break;
                        }
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(apprenticeshipDetails.DateOfBirth), $"The Date of birth is not valid");
            }
        }

        private IEnumerable<DomainError> BuildStartDateValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (request.StartDate.HasValue)
            {
                if (request.StartDate.Value != apprenticeshipDetails.StartDate.Value)
                {
                    if (request.StartDate.Value > _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
                    {
                        yield return new DomainError(nameof(apprenticeshipDetails.StartDate),
                            "The start date must be no later than one year after the end of the current teaching year");
                        yield break;
                    }

                    if (request.StartDate.Value < _academicYearDateProvider.CurrentAcademicYearStartDate &&
             _currentDateTime.UtcNow > _academicYearDateProvider.LastAcademicYearFundingPeriod)
                    {
                        yield return new DomainError(nameof(apprenticeshipDetails.StartDate),
                          $"The earliest start date you can use is { _academicYearDateProvider.CurrentAcademicYearStartDate.ToGdsFormatShortMonthWithoutDay()}");
                        yield break;
                    }

                    if (!string.IsNullOrWhiteSpace(request.CourseCode))
                    {
                        // Get Earliest Start Date and Latest End Date for a Standard across Versions
                        var result = _mediator.Send(new GetTrainingProgrammeQuery
                        {
                            Id = request.CourseCode
                        }).Result;

                        var courseStartedBeforeDas = result.TrainingProgramme != null &&
                                                           (!result.TrainingProgramme.EffectiveFrom.HasValue ||
                                                            result.TrainingProgramme.EffectiveFrom.Value < Constants.DasStartDate);

                        var trainingProgrammeStatus = GetStatusOn(request.StartDate.Value, result);

                        if ((request.StartDate.Value < Constants.DasStartDate) && (!trainingProgrammeStatus.HasValue || courseStartedBeforeDas))
                        {
                            yield return new DomainError(nameof(request.StartDate), "The start date must not be earlier than May 2017");
                            yield break;
                        }

                        if (trainingProgrammeStatus.HasValue && trainingProgrammeStatus.Value != TrainingProgrammeStatus.Active)
                        {
                            var suffix = trainingProgrammeStatus == TrainingProgrammeStatus.Pending
                                ? $"after {result.TrainingProgramme.EffectiveFrom.Value.AddMonths(-1):MM yyyy}"
                                : $"before {result.TrainingProgramme.EffectiveTo.Value.AddMonths(1):MM yyyy}";

                            var errorMessage = $"This training course is only available to apprentices with a start date {suffix}";

                            yield return new DomainError(nameof(request.StartDate), errorMessage);
                            yield break;
                        }

                        if (trainingProgrammeStatus.HasValue && apprenticeshipDetails.Cohort.TransferSenderId.HasValue
                            && request.StartDate.Value < Constants.TransferFeatureStartDate)
                        {
                            var errorMessage = $"Apprentices funded through a transfer can't start earlier than May 2018";

                            yield return new DomainError(nameof(request.StartDate), errorMessage);
                        }
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(request.StartDate), $"The start date is not valid");
            }
        }

        private IEnumerable<DomainError> BuildEndDateValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            if (request.EndDate.HasValue)
            {
                if (request.EndDate.Value != apprenticeshipDetails.EndDate.Value)
                {
                    if (request.EndDate < Constants.DasStartDate)
                    {
                        yield return new DomainError(nameof(request.EndDate), "The end date must not be earlier than May 2017");
                        yield break;
                    }
                }

                if (request.StartDate.HasValue)
                {
                    if (request.EndDate <= request.StartDate)
                    {
                        yield return new DomainError(nameof(request.EndDate), "The end date must not be on or before the start date");
                    }
                }
            }
            else
            {
                yield return new DomainError(nameof(request.EndDate), $"The end date is not valid");
            }
        }

        private IEnumerable<DomainError> BuildFlexibleEmploymentValidationFailures(EditApprenticeshipValidationRequest apprenticeshipRequest, Apprenticeship apprenticeshipDetails)
        {
            if(apprenticeshipRequest.DeliveryModel == DeliveryModel.PortableFlexiJob)
            {
                foreach (var failure in BuildFlexibleEmploymentPriceValidationFailures(apprenticeshipRequest))
                {
                    yield return failure;
                }

                foreach (var failure in BuildFlexibleEmploymentDateValidationFailures(apprenticeshipRequest))
                {
                    yield return failure;
                }
            }
        }

        private IEnumerable<DomainError> BuildFlexibleEmploymentDateValidationFailures(EditApprenticeshipValidationRequest request)
        {
            if (request.EmploymentEndDate == null)
            {
                yield return new DomainError(nameof(request.EmploymentEndDate), "You must add the employment end date");
            }
            else if (request.EmploymentEndDate.Value < request.StartDate?.AddMonths(3))
            {
                yield return new DomainError(nameof(request.EmploymentEndDate), "This date must be at least 3 months later than the planned apprenticeship training start date");
            }
            if (request.EmploymentEndDate > request.EndDate)
            {
                yield return new DomainError(nameof(request.EmploymentEndDate), "This date must not be later than the projected apprenticeship training end date");
            }
        }

        private IEnumerable<DomainError> BuildFlexibleEmploymentPriceValidationFailures(EditApprenticeshipValidationRequest request)
        {
            if (request.EmploymentPrice == null || request.EmploymentPrice <= 0)
            {
                yield return new DomainError(nameof(request.EmploymentPrice), "You must add the agreed price for this employment");
            }

            if (request.EmploymentPrice > Constants.MaximumApprenticeshipCost)
            {
                yield return new DomainError(nameof(request.EmploymentPrice), "The agreed price for this employment must be £100,000 or less");
            }

            if (request.Cost.GetValueOrDefault() <= 0)
            {
                yield break;
            }

            if (request.EmploymentPrice > request.Cost)
            {
                yield return new DomainError(nameof(request.EmploymentPrice), "This price must not be more than the total agreed apprenticeship price");
            }
        }

        private static TrainingProgrammeStatus? GetStatusOn(DateTime startDate, GetTrainingProgrammeQueryResult result)
        {
            var dateOnly = startDate;

            if (result.TrainingProgramme.EffectiveFrom.HasValue && result.TrainingProgramme.EffectiveFrom.Value.FirstOfMonth() > dateOnly)
                return TrainingProgrammeStatus.Pending;

            if (!result.TrainingProgramme.EffectiveTo.HasValue || result.TrainingProgramme.EffectiveTo.Value >= dateOnly)
                return TrainingProgrammeStatus.Active;

                return TrainingProgrammeStatus.Expired;
        }

        public int? AgeOnStartDate(DateTime? dateOfBirth, DateTime? newStartDate)
        {
            var startDate = newStartDate.Value;
            var age = startDate.Year - dateOfBirth.Value.Year;

            if ((dateOfBirth.Value.Month > startDate.Month) ||
                (dateOfBirth.Value.Month == startDate.Month &&
                 dateOfBirth.Value.Day > startDate.Day))
                age--;

            return age;
        }

        private bool IsLockedForUpdate(Apprenticeship apprenticeship)
        {
            return (MapApprenticeshipStatus(apprenticeship) == ApprenticeshipStatus.Live &&
                                    (apprenticeship.HasHadDataLockSuccess || _currentDateTime.UtcNow > _academicYearDateProvider.LastAcademicYearFundingPeriod &&
                                    !IsWithInFundingPeriod(apprenticeship.StartDate.Value)))
                                   ||
                                   (apprenticeship.Cohort.TransferSenderId.HasValue
                                    && apprenticeship.HasHadDataLockSuccess && MapApprenticeshipStatus(apprenticeship) == ApprenticeshipStatus.WaitingToStart);
        }

        private bool IsEndDateLocked(Apprenticeship apprenticeship)
        {
            var result = IsLockedForUpdate(apprenticeship);
            if (apprenticeship.HasHadDataLockSuccess)
            {
                result = MapApprenticeshipStatus(apprenticeship) == ApprenticeshipStatus.WaitingToStart;
            }

            return result;
        }

        private bool IsUpdateLockedForStartDateAndCourse(Apprenticeship apprenticeship)
        {
            return apprenticeship.Cohort.TransferSenderId.HasValue && !apprenticeship.HasHadDataLockSuccess;
        }

        private bool IsWithInFundingPeriod(DateTime trainingStartDate)
        {
            if (trainingStartDate < _academicYearDateProvider.CurrentAcademicYearStartDate &&
                 _currentDateTime.UtcNow > _academicYearDateProvider.LastAcademicYearFundingPeriod)
            {
                return false;
            }

            return true;
        }

        private ApprenticeshipStatus MapApprenticeshipStatus(Apprenticeship source)
        {
            var now = new DateTime(_currentDateTime.UtcNow.Year, _currentDateTime.UtcNow.Month, 1);
            var waitingToStart = source.StartDate.HasValue && source.StartDate.Value > now;

            switch (source.PaymentStatus)
            {
                case PaymentStatus.Active:
                    return waitingToStart ? ApprenticeshipStatus.WaitingToStart : ApprenticeshipStatus.Live;
                case PaymentStatus.Paused:
                    return ApprenticeshipStatus.Paused;
                case PaymentStatus.Withdrawn:
                    return ApprenticeshipStatus.Stopped;
                case PaymentStatus.Completed:
                    return ApprenticeshipStatus.Completed;
                default:
                    return ApprenticeshipStatus.Unknown;
            }
        }
    }
}
