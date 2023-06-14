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
                var result = await _cacheStorageService.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(cacheKey);

                if (result != null)
                {
                    return result;
                }
            }

            var queryResult = new GetApprenticeshipsFilterValuesQueryResult
            {
                StartDates = await GetDistinctStartDates(query, cancellationToken),
                EndDates = await GetDistinctEndDates(query, cancellationToken),
                CourseNames = await GetDistinctCourseNames(query, cancellationToken),
                Sectors = await GetDistinctSectors(query, cancellationToken)
            };

            if (query.ProviderId.HasValue)
            {
                queryResult.EmployerNames = await GetDistinctEmployerNames(query, cancellationToken);
            }
            else if (query.EmployerAccountId.HasValue)
            {
                queryResult.ProviderNames = await GetDistinctProviderNames(query, cancellationToken);
            }

            await _cacheStorageService.SaveToCache(cacheKey, queryResult, 1);

            return queryResult;
        }

        private async Task<List<string>> GetDistinctSectors(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            var standardUIds = await _dbContext.Apprenticeships
                .WithProviderOrEmployerId(request).Where(x => x.ProgrammeType == 0).Select(x => x.StandardUId).Distinct().ToListAsync(cancellationToken);

            var sectors = await _dbContext.Standards.Where(x => standardUIds.Contains(x.StandardUId)).Select(x => x.Route).ToListAsync(cancellationToken);

            return sectors;
        }

        private async Task<List<string>> GetDistinctEmployerNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Apprenticeships
                .WithProviderOrEmployerId(request)
                .OrderBy(apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name)
                .Select(apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private async Task<List<string>> GetDistinctProviderNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Apprenticeships
                .WithProviderOrEmployerId(request)
                .OrderBy(apprenticeship => apprenticeship.Cohort.Provider.Name)
                .Select(apprenticeship => apprenticeship.Cohort.Provider.Name)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private async Task<List<string>> GetDistinctCourseNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Apprenticeships
                .WithProviderOrEmployerId(request)
                .OrderBy(apprenticeship => apprenticeship.CourseName)
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private async Task<List<DateTime>> GetDistinctStartDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Apprenticeships
                .WithProviderOrEmployerId(request)
                .Where(apprenticeship => apprenticeship.StartDate.HasValue)
                .OrderBy(apprenticeship => apprenticeship.StartDate)
                .Select(apprenticeship => apprenticeship.StartDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private async Task<List<DateTime>> GetDistinctEndDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return await _dbContext.Apprenticeships
                .WithProviderOrEmployerId(request)
                .Where(apprenticeship => apprenticeship.EndDate.HasValue)
                .OrderBy(apprenticeship => apprenticeship.EndDate)
                .Select(apprenticeship => apprenticeship.EndDate.Value)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}
