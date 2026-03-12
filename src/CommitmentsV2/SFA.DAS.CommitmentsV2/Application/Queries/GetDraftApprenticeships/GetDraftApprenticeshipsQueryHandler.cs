using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships;

public class GetDraftApprenticeshipsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
    : IRequestHandler<GetDraftApprenticeshipsQuery, GetDraftApprenticeshipsQueryResult>
{
    public async Task<GetDraftApprenticeshipsQueryResult> Handle(GetDraftApprenticeshipsQuery query, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;
        var cohortQuery = db.Cohorts
        .Include(x => x.Apprenticeships).ThenInclude(x => x.FlexibleEmployment)
        .Include(x => x.Apprenticeships).ThenInclude(x => x.PriorLearning)
        .Where(x => x.Id == query.CohortId)
        .Select(x => new { DraftApprenticeships = x.Apprenticeships });
        
        var cohort = await cohortQuery.SingleOrDefaultAsync();

        var courseIds = cohort?.DraftApprenticeships.Where(x => x.StandardUId != null).Select(x => x.StandardUId).Distinct().ToList();
        var standards = db.Standards.Where(x => courseIds.Contains(x.StandardUId)).ToList();

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
                RecognisingPriorLearningExtendedStillNeedsToBeConsidered = a.RecognisingPriorLearningExtendedStillNeedsToBeConsidered,
                EmployerHasEditedCost = a.EmployerHasEditedCost,
                EmailAddressConfirmed = a.EmailAddressConfirmed,
                DurationReducedByHours = a.PriorLearning?.DurationReducedByHours,
                HasLearnerDataChanges = a.HasLearnerDataChanges,
                LastLearnerDataSync = a.LastLearnerDataSync,
                LearnerDataId = a.LearnerDataId,
                ApprenticeshipType = GetApprenticeshipType(a.StandardUId)
            }).ToList()
        };

        string GetApprenticeshipType(string standardUId)
        {
            string retVal = null;
            if (string.IsNullOrEmpty(standardUId))
                return retVal;

            var standard = standards.FirstOrDefault(s => s.StandardUId == standardUId);
            if(standard == null)
                return retVal;
            else
                return standard.ApprenticeshipType ?? "Apprenticeship";
        }
    }
}