using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQueryHandler : IRequestHandler<GetApprovedApprenticesFilterValuesQuery, GetApprovedApprenticesFilterValuesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprovedApprenticesFilterValuesQueryHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprovedApprenticesFilterValuesResponse> Handle(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
           var dbTasks = new []{
                GetDistinctEmployerNames(request, cancellationToken),
                GetDistinctCourseNames(request, cancellationToken)
            };

            Task.WaitAll(dbTasks.ToArray<Task>());
            
            return await Task.FromResult(new GetApprovedApprenticesFilterValuesResponse
            {
                EmployerNames = dbTasks[0].Result,
                CourseNames = dbTasks[1].Result,
            });
        }

        private Task<List<string>> GetDistinctEmployerNames(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctCourseNames(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
