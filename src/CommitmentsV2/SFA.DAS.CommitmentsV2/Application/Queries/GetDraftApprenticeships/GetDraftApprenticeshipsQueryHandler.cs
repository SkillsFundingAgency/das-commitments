using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;

public class GetDraftApprenticeshipsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<GetDraftApprenticeshipsQuery, GetDraftApprenticeshipsQueryResult>
{
    public async Task<GetDraftApprenticeshipsQueryResult> Handle(GetDraftApprenticeshipsQuery query, CancellationToken cancellationToken)
    {
        var cohort = await dbContext.Value.Cohorts
            .Include(x => x.Apprenticeships).ThenInclude(x => x.FlexibleEmployment)
            .Include(x => x.Apprenticeships).ThenInclude(x => x.PriorLearning)
            .Where(x => x.Id == query.CohortId)
            .Select(x => new { DraftApprenticeships = x.Apprenticeships })
            .SingleOrDefaultAsync(cancellationToken);

        return new GetDraftApprenticeshipsQueryResult
        {
            DraftApprenticeships = cohort?.DraftApprenticeships.Select(a => new DraftApprenticeshipDto
            {
                Id = a.Id,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email,
                Cost = (int?)a.Cost,
                TrainingPrice = a.TrainingPrice,
                EndPointAssessmentPrice = a.EndPointAssessmentPrice,
                CourseCode = a.CourseCode,
                CourseName = a.CourseName,
                DeliveryModel = a.DeliveryModel ?? DeliveryModel.Regular,
                DateOfBirth = a.DateOfBirth,
                StartDate = a.StartDate,
                ActualStartDate = a.ActualStartDate,
                EndDate = a.EndDate,
                Uln = a.Uln,
                OriginalStartDate = a.OriginalStartDate,
                EmploymentEndDate = a.FlexibleEmployment?.EmploymentEndDate,
                EmploymentPrice = a.FlexibleEmployment?.EmploymentPrice,
                RecognisePriorLearning = a.RecognisePriorLearning,
                DurationReducedBy = a.PriorLearning?.DurationReducedBy,
                PriceReducedBy = a.PriorLearning?.PriceReducedBy,
                RecognisingPriorLearningStillNeedsToBeConsidered = a.RecognisingPriorLearningStillNeedsToBeConsidered,
                RecognisingPriorLearningExtendedStillNeedsToBeConsidered = a.RecognisingPriorLearningExtendedStillNeedsToBeConsidered,
                IsOnFlexiPaymentPilot = a.IsOnFlexiPaymentPilot,
                EmployerHasEditedCost = a.EmployerHasEditedCost,
                EmailAddressConfirmed = a.EmailAddressConfirmed,
                DurationReducedByHours = a.PriorLearning?.DurationReducedByHours,
            }).ToList()
        };
    }
}