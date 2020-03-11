using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryHandler : IRequestHandler<GetApprenticeshipsFilterValuesQuery, GetApprenticeshipsFilterValuesQueryResult>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;
        private readonly ICacheStorageService _cacheStorageService;

        public GetApprenticeshipsFilterValuesQueryHandler(IProviderCommitmentsDbContext dbContext, ICacheStorageService cacheStorageService)
        {
            _dbContext = dbContext;
            _cacheStorageService = cacheStorageService;
        }

        public async Task<GetApprenticeshipsFilterValuesQueryResult> Handle(GetApprenticeshipsFilterValuesQuery query, CancellationToken cancellationToken)
        {
            var cacheKey = $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}";
            var result = await _cacheStorageService.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(cacheKey);

            if(result != null)
            {
                return result;
            }

            var stringDbTasks = new []{
                GetDistinctEmployerNames(query, cancellationToken),
                GetDistinctCourseNames(query, cancellationToken)
            };

            var dateDbTasks = new[]{
               GetDistinctStartDates(query, cancellationToken),
               GetDistinctEndDates(query, cancellationToken)
            };

            var dbTasks = new List<Task>();
            dbTasks.AddRange(stringDbTasks);
            dbTasks.AddRange(dateDbTasks);

            Task.WaitAll(dbTasks.ToArray<Task>());

            var queryResult = await Task.FromResult(new GetApprenticeshipsFilterValuesQueryResult
            {
                EmployerNames = stringDbTasks[0].Result,
                CourseNames = stringDbTasks[1].Result,
                StartDates = dateDbTasks[0].Result,
                EndDates = dateDbTasks[1].Result
            });

            await _cacheStorageService.SaveToCache(cacheKey, queryResult, 1);

            return queryResult;
        }

        private Task<List<string>> GetDistinctEmployerNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .OrderBy(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Select(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctCourseNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId)
                .OrderBy(apprenticeship => apprenticeship.CourseName)
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<DateTime>> GetDistinctStartDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId 
                                         && apprenticeship.StartDate.HasValue)
                .OrderBy(apprenticeship => apprenticeship.StartDate)
                .Select(apprenticeship => apprenticeship.StartDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<DateTime>> GetDistinctEndDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId &&
                                         apprenticeship.EndDate.HasValue)
                .OrderBy(apprenticeship => apprenticeship.EndDate)
                .Select(apprenticeship => apprenticeship.EndDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
