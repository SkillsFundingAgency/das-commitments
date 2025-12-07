using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.EmailValidationService;

namespace SFA.DAS.CommitmentsV2.Services;

public class ViewEditDraftApprenticeshipEmailValidationService : IViewEditDraftApprenticeshipEmailValidationService
{

    private readonly IProviderCommitmentsDbContext _context;
    private readonly IOverlapCheckService _overlapCheckService;

    public ViewEditDraftApprenticeshipEmailValidationService(IProviderCommitmentsDbContext context,
        IMediator mediator,
        IOverlapCheckService overlapCheckService)
    {
        _context = context;
        _overlapCheckService = overlapCheckService;
    }

    public async Task<ViewEditDraftApprenticeshipEmailValidationResult> Validate(ViewEditDraftApprenticeshipEmailValidationRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<DomainError>();
        var apprenticeship = _context.DraftApprenticeships
            .Include(y => y.Cohort)
            .FirstOrDefault(x => x.Id == request.DraftApprenticeshipId);
        if (apprenticeship == null)
        {
            return null;
        }


        if (errors.Count == 0)
        {
            errors.AddRange(BuildEmailValidationFailures(request, apprenticeship));
        }

        if (errors.Count == 0)
        {
            var overlapError = await EmailOverlapValidationFailures(request, apprenticeship);

            if (overlapError != null)
            {
                errors.Add(overlapError);
            }
        }

        return new ViewEditDraftApprenticeshipEmailValidationResult()
        {
            Errors = errors
        };
    }
    private async Task<DomainError> EmailOverlapValidationFailures(ViewEditDraftApprenticeshipEmailValidationRequest request, DraftApprenticeship apprenticeshipDetails)
    {
        var emailMatches = request.Email == apprenticeshipDetails.Email;
        var startDateMatches = request.StartDate == apprenticeshipDetails.StartDate.Value.ToShortDateString();
        var endDateMatches = request.EndDate == apprenticeshipDetails.EndDate.Value.ToShortDateString();

        bool NoChangesRequested() => (emailMatches && startDateMatches && endDateMatches);

        if (string.IsNullOrWhiteSpace(request.Email))
            return null;

        if (NoChangesRequested())
            return null;

        var startDate = DateTime.Parse(request.StartDate).Date;
        var endDate = DateTime.Parse(request.EndDate).Date;

        var range = startDate.To(endDate);

        var overlap = await _overlapCheckService.CheckForEmailOverlaps(request.Email, range, request.DraftApprenticeshipId, null, CancellationToken.None);

        if (overlap != null)
        {
            return new DomainError(nameof(request.Email), overlap.BuildErrorMessage());
        }

        return null;
    }

    private IEnumerable<DomainError> BuildEmailValidationFailures(ViewEditDraftApprenticeshipEmailValidationRequest request, DraftApprenticeship apprenticeshipDetails)
    {
        if (apprenticeshipDetails.Email != null && string.IsNullOrWhiteSpace(request.Email))
        {
            yield return new DomainError(nameof(request.Email), "Email address cannot be blank");
        }

        if (apprenticeshipDetails.Email == null && !string.IsNullOrWhiteSpace(request.Email) && apprenticeshipDetails.Cohort.EmployerAndProviderApprovedOn < new DateTime(2021, 09, 10))
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
}
