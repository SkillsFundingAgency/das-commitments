using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship
{
    public class GetDraftApprenticeshipQueryHandler : IRequestHandler<GetDraftApprenticeshipQuery, GetDraftApprenticeshipQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IAuthenticationService _authenticationService;

        public GetDraftApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IAuthenticationService authenticationService)
        {
            _dbContext = dbContext; 
            _authenticationService = authenticationService;
        }

        public async Task<GetDraftApprenticeshipQueryResult> Handle(GetDraftApprenticeshipQuery request, CancellationToken cancellationToken)
        {
            var requestingParty = _authenticationService.GetUserParty();

            var query = _dbContext.Value.DraftApprenticeships
                .Include(x => x.PriorLearning)
                .Where(x => x.Id == request.DraftApprenticeshipId && x.CommitmentId == request.CohortId);

            var x = await query.Select(draft => new GetDraftApprenticeshipQueryResult
            {
                CourseCode = draft.CourseCode,
                TrainingCourseVersion = draft.TrainingCourseVersion,
                TrainingCourseVersionConfirmed = draft.TrainingCourseVersionConfirmed,
                TrainingCourseName = draft.CourseName,
                TrainingCourseOption = draft.TrainingCourseOption,
                StandardUId = draft.StandardUId,
                DeliveryModel = draft.DeliveryModel,
                StartDate = draft.StartDate,
                ActualStartDate = draft.ActualStartDate,
                Id = draft.Id,
                Cost = (int?) draft.Cost,
                DateOfBirth = draft.DateOfBirth,
                EndDate = draft.EndDate,
                FirstName = draft.FirstName,
                LastName = draft.LastName,
                Email = draft.Email,
                Reference = requestingParty == Party.Provider ? draft.ProviderRef : draft.EmployerRef,
                EmployerReference = draft.EmployerRef,
                ProviderReference = draft.ProviderRef,
                ReservationId = draft.ReservationId,
                Uln = draft.Uln,
                IsContinuation = draft.ContinuationOfId.HasValue,
                ContinuationOfId = draft.ContinuationOfId,
                OriginalStartDate = draft.OriginalStartDate,
                HasStandardOptions = !string.IsNullOrEmpty(draft.StandardUId) && _dbContext.Value.StandardOptions.Any(c => c.StandardUId.Equals(draft.StandardUId)),
                EmploymentEndDate = draft.FlexibleEmployment != null ? draft.FlexibleEmployment.EmploymentEndDate : null,
                EmploymentPrice = draft.FlexibleEmployment != null ? draft.FlexibleEmployment.EmploymentPrice : null,
                RecognisePriorLearning = draft.RecognisePriorLearning,
                DurationReducedBy = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedBy : null,
                PriceReducedBy = draft.PriorLearning != null ? draft.PriorLearning.PriceReducedBy : null,
                DurationReducedByHours = draft.PriorLearning != null ? draft.PriorLearning.DurationReducedByHours : null,
                WeightageReducedBy = draft.PriorLearning != null ? draft.PriorLearning.WeightageReducedBy : null,
                ReasonForRplReduction = draft.PriorLearning != null ? draft.PriorLearning.ReasonForRplReduction : null,
                QualificationsForRplReduction = draft.PriorLearning != null ? draft.PriorLearning.QualificationsForRplReduction : null,
                RecognisingPriorLearningStillNeedsToBeConsidered = draft.RecognisingPriorLearningStillNeedsToBeConsidered,
                RecognisingPriorLearningExtendedStillNeedsToBeConsidered = draft.RecognisingPriorLearningExtendedStillNeedsToBeConsidered,
                IsOnFlexiPaymentPilot = draft.IsOnFlexiPaymentPilot,
                EmailAddressConfirmed = draft.EmailAddressConfirmed
            }).SingleOrDefaultAsync(cancellationToken);

            return x;
        }
    }
}
