using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryHandler : IRequestHandler<GetApprenticeshipsFilterValuesQuery, GetApprenticeshipsFilterValuesQueryResult>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprenticeshipsFilterValuesQueryHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprenticeshipsFilterValuesQueryResult> Handle(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            var stringDbTasks = new []{
                GetDistinctEmployerNames(request, cancellationToken),
                GetDistinctCourseNames(request, cancellationToken),
                GetDistinctStatuses(request, cancellationToken)
            };

            var dateDbTasks = new[]{
               GetDistinctStartDates(request, cancellationToken),
               GetDistinctEndDates(request, cancellationToken)
            };

            var dbTasks = new List<Task>();
            dbTasks.AddRange(stringDbTasks);
            dbTasks.AddRange(dateDbTasks);

            Task.WaitAll(dbTasks.ToArray<Task>());

            return await Task.FromResult(new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = stringDbTasks[0].Result,
                CourseNames = stringDbTasks[1].Result,
                Statuses = stringDbTasks[2].Result,
                StartDates = dateDbTasks[0].Result,
                EndDates = dateDbTasks[1].Result
            });
        }

        private Task<List<string>> GetDistinctEmployerNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .Select(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctCourseNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctStatuses(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .Select(apprenticeship => apprenticeship.Cohort.CommitmentStatus)
                .Distinct()
                .Select(status => Enum.GetName(typeof(CommitmentStatus), status))
                .ToListAsync(cancellationToken);
        }

        private Task<List<DateTime>> GetDistinctStartDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId 
                                         && apprenticeship.StartDate.HasValue)
                .Select(apprenticeship => apprenticeship.StartDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<DateTime>> GetDistinctEndDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId &&
                                         apprenticeship.EndDate.HasValue)
                .Select(apprenticeship => apprenticeship.EndDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
