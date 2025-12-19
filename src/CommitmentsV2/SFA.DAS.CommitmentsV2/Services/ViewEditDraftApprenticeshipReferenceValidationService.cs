using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services;

public class ViewEditDraftApprenticeshipReferenceValidationService: IViewEditDraftApprenticeshipReferenceValidationService
{
    private readonly IProviderCommitmentsDbContext _context;

    public ViewEditDraftApprenticeshipReferenceValidationService(IProviderCommitmentsDbContext context,
        IMediator mediator)
    {
        _context = context;
    }

    public async Task<ViewEditDraftApprenticeshipReferenceValidationResult> Validate(ViewEditDraftApprenticeshipReferenceValidationRequest request, CancellationToken cancellationToken)
    {
        var errors = new List<DomainError>();

        var cohort = _context.Cohorts.FirstOrDefault(x => x.Id == request.CohortId);
        var apprenticeship = _context.DraftApprenticeships
           .Include(y => y.Cohort)
           .FirstOrDefault(x => x.Id == request.DraftApprenticeshipId);

        if (cohort == null)
        {
            throw new ApplicationException($"CohortId {request.CohortId} not found");
        }

        if (apprenticeship == null)
        {
            throw new ApplicationException($"ApprenticeshipId {request.DraftApprenticeshipId} not found");
        }

        errors.AddRange(BuildRefValidationFailures(request, apprenticeship, cohort));

        return new ViewEditDraftApprenticeshipReferenceValidationResult()
        {
            Errors = errors
        };
    }   

    private IEnumerable<DomainError> BuildRefValidationFailures(ViewEditDraftApprenticeshipReferenceValidationRequest request, DraftApprenticeship apprenticeshipDetails, Cohort cohort)
    {
        if (cohort.WithParty != request.Party)
        {
            yield return new DomainError(nameof(request.Reference), "You cannot update the Reference value as the cohort is not assigned to you");
        }

        if (request.Reference != null && request.Reference.Length > 20)
        {
            yield return new DomainError(nameof(request.Reference), "The Reference must be 20 characters or fewer");
        }
    }
}