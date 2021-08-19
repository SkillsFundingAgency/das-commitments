using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsQueryHandler : IRequestHandler<GetDraftApprenticeshipsQuery, GetDraftApprenticeshipsQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDraftApprenticeshipsQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetDraftApprenticeshipsQueryResult> Handle(GetDraftApprenticeshipsQuery query, CancellationToken cancellationToken)
        {
            var cohort = _dbContext.Value.Cohorts
                .Where(x => x.Id == query.CohortId)
                .Select(x => new { DraftApprenticeships = x.Apprenticeships})
                .SingleOrDefault();

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
                    DateOfBirth =  a.DateOfBirth,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Uln = a.Uln,
                    OriginalStartDate = a.OriginalStartDate,
                    HasStandardOptions = !string.IsNullOrEmpty(a.StandardUId) && _dbContext.Value.StandardOptions.Any(c=>c.StandardUId.Equals(a.StandardUId))
                }).ToList()
            });
        }
    }
}
