using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsQueryHandler : IRequestHandler<GetDraftApprenticeshipsQuery, GetDraftApprenticeshipsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IFeatureTogglesService<FeatureToggle> _featureTogglesService;

        public GetDraftApprenticeshipsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, IFeatureTogglesService<FeatureToggle> featureTogglesService)
        {
            _dbContext = dbContext;
            _featureTogglesService = featureTogglesService;
        }

        public Task<GetDraftApprenticeshipsQueryResult> Handle(GetDraftApprenticeshipsQuery query, CancellationToken cancellationToken)
        {
            var cohort = _dbContext.Value.Cohorts
                .Include(x => x.Apprenticeships).ThenInclude(x => x.FlexibleEmployment)
                .Include(x => x.Apprenticeships).ThenInclude(x => x.PriorLearning)
                .Where(x => x.Id == query.CohortId)
                .Select(x => new { DraftApprenticeships = x.Apprenticeships})
                .SingleOrDefault();

            var isRplRequired = _featureTogglesService.GetFeatureToggle(Constants.RecognitionOfPriorLearningFeature).IsEnabled;

            return Task.FromResult(new GetDraftApprenticeshipsQueryResult
            {
                DraftApprenticeships = cohort?.DraftApprenticeships.Select(a => new DraftApprenticeshipDto
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Email = a.Email,
                    Cost = (int?) a.Cost,
                    CourseCode = a.CourseCode,
                    CourseName = a.CourseName,
                    DeliveryModel = a.DeliveryModel ?? DeliveryModel.Regular,
                    DateOfBirth =  a.DateOfBirth,
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
                    RecognisingPriorLearningStillNeedsToBeConsidered = isRplRequired && a.RecognisingPriorLearningStillNeedsToBeConsidered,
                    IsOnFlexiPaymentPilot = a.IsOnFlexiPaymentPilot,
                    EmailAddressConfirmed = a.EmailAddressConfirmed,
                    DurationReducedByHours = a.DurationReducedByHours,
                    WeightageReducedBy = a.WeightageReducedBy,
                    Qualification = a.Qualification,
                    Reason = a.Reason
                }).ToList()
            });
        }
    }
}
