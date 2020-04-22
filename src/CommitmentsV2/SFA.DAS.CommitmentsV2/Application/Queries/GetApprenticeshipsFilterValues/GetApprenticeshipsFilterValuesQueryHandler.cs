using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;

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
            var cacheKey = "";

            if (query.ProviderId.HasValue)
            {
                cacheKey = $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.ProviderId}";
            }

            if (query.EmployerAccountId.HasValue)
            {
                cacheKey = $"{nameof(GetApprenticeshipsFilterValuesQueryResult)}-{query.EmployerAccountId}";
            }

            if (!string.IsNullOrWhiteSpace(cacheKey))
            {
                var result =
                    await _cacheStorageService.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(cacheKey);

                if (result != null)
                {
                    return result;
                }
            }

            var stringDbTasks = new List<Task<List<string>>>();
            
            if (query.ProviderId.HasValue)
            {
                stringDbTasks.Add(GetDistinctEmployerNames(query, cancellationToken));
            }

            if (query.EmployerAccountId.HasValue)
            {
                stringDbTasks.Add(GetDistinctProviderNames(query, cancellationToken));
            }

            stringDbTasks.Add( GetDistinctCourseNames(query, cancellationToken));

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
                StartDates = dateDbTasks[0].Result,
                EndDates = dateDbTasks[1].Result
            });

            if (query.ProviderId.HasValue)
            {
                queryResult.EmployerNames = stringDbTasks[0].Result;
                queryResult.CourseNames = stringDbTasks[1].Result;
            }
            else if (query.EmployerAccountId.HasValue)
            {
                queryResult.ProviderNames = stringDbTasks[0].Result;
                queryResult.CourseNames = stringDbTasks[1].Result;
            }
            else
            {
                queryResult.CourseNames = stringDbTasks[1].Result;
            }

            await _cacheStorageService.SaveToCache(cacheKey, queryResult, 1);

            return queryResult;
        }

        private Task<List<string>> GetDistinctEmployerNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .WithId(request)
				.OrderBy(apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name)
                .Select(apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
                
        private Task<List<string>> GetDistinctProviderNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .WithId(request)
				.OrderBy(apprenticeship => apprenticeship.Cohort.Provider.Name)
                .Select(apprenticeship => apprenticeship.Cohort.Provider.Name)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctCourseNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .WithId(request)
                .OrderBy(apprenticeship => apprenticeship.CourseName)
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<DateTime>> GetDistinctStartDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .WithId(request)
                .Where(apprenticeship => apprenticeship.StartDate.HasValue)
                .OrderBy(apprenticeship => apprenticeship.StartDate)
                .Select(apprenticeship => apprenticeship.StartDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<DateTime>> GetDistinctEndDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .WithId(request)
                .Where(apprenticeship => apprenticeship.EndDate.HasValue)
                .OrderBy(apprenticeship => apprenticeship.EndDate)
                .Select(apprenticeship => apprenticeship.EndDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
