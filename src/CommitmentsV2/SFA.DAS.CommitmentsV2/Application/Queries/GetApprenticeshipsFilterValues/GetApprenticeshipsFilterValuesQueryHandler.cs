using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues;

public class GetApprenticeshipsFilterValuesQueryHandler(
    IProviderCommitmentsDbContext dbContext,
    ICacheStorageService cacheStorageService)
    : IRequestHandler<GetApprenticeshipsFilterValuesQuery, GetApprenticeshipsFilterValuesQueryResult>
{
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
            var result = await cacheStorageService.RetrieveFromCache<GetApprenticeshipsFilterValuesQueryResult>(cacheKey);

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

        await cacheStorageService.SaveToCache(cacheKey, queryResult, 1);

        return queryResult;
    }

    private async Task<List<string>> GetDistinctSectors(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
    {
        var standardUIds = await dbContext.Apprenticeships
            .WithProviderOrEmployerId(request).Where(x => x.ProgrammeType == ProgrammeType.Standard).Select(x => x.StandardUId).Distinct().ToListAsync(cancellationToken);

        var sectors = await dbContext.Standards.Where(x => standardUIds.Contains(x.StandardUId)).Select(x => x.Route).Distinct().ToListAsync(cancellationToken);

        return sectors;
    }

    private async Task<List<string>> GetDistinctEmployerNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Apprenticeships
            .WithProviderOrEmployerId(request)
            .OrderBy(apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name)
            .Select(apprenticeship => apprenticeship.Cohort.AccountLegalEntity.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> GetDistinctProviderNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Apprenticeships
            .WithProviderOrEmployerId(request)
            .OrderBy(apprenticeship => apprenticeship.Cohort.Provider.Name)
            .Select(apprenticeship => apprenticeship.Cohort.Provider.Name)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> GetDistinctCourseNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Apprenticeships
            .WithProviderOrEmployerId(request)
            .OrderBy(apprenticeship => apprenticeship.CourseName)
            .Select(apprenticeship => apprenticeship.CourseName)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DateTime>> GetDistinctStartDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Apprenticeships
            .WithProviderOrEmployerId(request)
            .Where(apprenticeship => apprenticeship.StartDate.HasValue)
            .OrderBy(apprenticeship => apprenticeship.StartDate)
            .Select(apprenticeship => apprenticeship.StartDate.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<List<DateTime>> GetDistinctEndDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Apprenticeships
            .WithProviderOrEmployerId(request)
            .Where(apprenticeship => apprenticeship.EndDate.HasValue)
            .OrderBy(apprenticeship => apprenticeship.EndDate)
            .Select(apprenticeship => apprenticeship.EndDate.Value)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}