using Azure.Core;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

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
        var apprenticeship = _context.DraftApprenticeships
           .Include(y => y.Cohort)
           .FirstOrDefault(x => x.Id == request.DraftApprenticeshipId);
        if (apprenticeship == null)
        {
            return null;
        }
        
         errors.AddRange(BuildProviderRefValidationFailures(request, apprenticeship));

        return new ViewEditDraftApprenticeshipReferenceValidationResult()
        {
            Errors = errors
        };
    }   

    private IEnumerable<DomainError> BuildProviderRefValidationFailures(ViewEditDraftApprenticeshipReferenceValidationRequest request, DraftApprenticeship apprenticeshipDetails)
    {
        if (request.Party == Party.Provider && request.Reference != apprenticeshipDetails.ProviderRef)
        {
            if (!string.IsNullOrWhiteSpace(request.Reference) && request.Reference.Length > 20)
            {
                yield return new DomainError(nameof(request.Reference), "The Reference must be 20 characters or fewer");
            }
        }
    }

}
