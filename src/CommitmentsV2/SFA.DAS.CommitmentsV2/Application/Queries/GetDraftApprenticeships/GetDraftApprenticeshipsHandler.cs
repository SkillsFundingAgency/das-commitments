using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsHandler : IRequestHandler<GetDraftApprenticeshipsRequest, GetDraftApprenticeshipsResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetDraftApprenticeshipsHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetDraftApprenticeshipsResult> Handle(GetDraftApprenticeshipsRequest request, CancellationToken cancellationToken)
        {
            var cohort = _dbContext.Value.Cohorts
                .Where(x => x.Id == request.CohortId)
                .Select(x => new { Cohort = x, DraftApprenticeship = x.Apprenticeships})
                .SingleOrDefault();

            return Task.FromResult(new GetDraftApprenticeshipsResult
            {
                DraftApprenticeships = cohort?.DraftApprenticeship.Select(a => new DraftApprenticeshipDto
                {
                    Id = a.Id,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    Cost = (int?) a.Cost,
                    CourseCode = a.CourseCode,
                    CourseName = a.CourseName,
                    DateOfBirth =  a.DateOfBirth,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    Uln = a.Uln
                }).ToList()
            });
        }
    }
}
