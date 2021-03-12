using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class EditApprenitceshipValidationService : IEditApprenticeshipValidationService
    {
        private readonly IProviderCommitmentsDbContext _context;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly IReservationValidationService _reservationValidationService;

        public EditApprenitceshipValidationService(IProviderCommitmentsDbContext context, IOverlapCheckService  overlapCheckService, IReservationValidationService reservationValidationService)
        {
            _context = context;
            _overlapCheckService = overlapCheckService;
            _reservationValidationService = reservationValidationService;
        }

        public Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request, CancellationToken cancellationToken)
        {
            var errors = new List<DomainError>();
            var apprenticeship = _context.Apprenticeships.FirstOrDefault(x => x.Id == request.ApprenticeshipId);
            errors.AddRange(BuildDateOfBirthValidationFailures(request, apprenticeship));


            return Task.FromResult(new EditApprenticeshipValidationResult()
            {
                Errors = errors
            });
        }


        private IEnumerable<DomainError> BuildDateOfBirthValidationFailures(EditApprenticeshipValidationRequest request, Apprenticeship apprenticeshipDetails)
        {
            var ageOnStartDate = AgeOnStartDate(request.DateOfBirth, request.StartDate, apprenticeshipDetails.StartDate.Value);
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

            if (apprenticeshipDetails.DateOfBirth.HasValue && apprenticeshipDetails.DateOfBirth < Constants.MinimumDateOfBirth)
            {
                yield return new DomainError(nameof(apprenticeshipDetails.DateOfBirth), $"The Date of birth is not valid");
            }
        }


        public int? AgeOnStartDate(DateTime? dateOfBirth, DateTime? newStartDate, DateTime existingStartDate)
        {
            if (dateOfBirth.HasValue)
            {
                var startDate = newStartDate.HasValue ? newStartDate.Value : existingStartDate;
                var age = startDate.Year - dateOfBirth.Value.Year;

                if ((dateOfBirth.Value.Month > startDate.Month) ||
                    (dateOfBirth.Value.Month == startDate.Month &&
                     dateOfBirth.Value.Day > startDate.Day))
                    age--;

                return age;
            }

            return null;
        }
    }
}
