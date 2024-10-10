using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;

public class ApprenticeshipSearch(IServiceProvider serviceProvider) : IApprenticeshipSearch
{
    public async Task<ApprenticeshipSearchResult> Find<T>(T searchParams)
    {
        var handler = serviceProvider.GetService<IApprenticeshipSearchService<T>>();

        return await handler.Find(searchParams);
    }
}