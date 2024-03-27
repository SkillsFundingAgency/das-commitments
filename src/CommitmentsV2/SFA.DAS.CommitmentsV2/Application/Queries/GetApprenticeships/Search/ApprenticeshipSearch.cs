using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search
{
    public class ApprenticeshipSearch : IApprenticeshipSearch
    {
        private readonly IServiceProvider _serviceProvider;

        public ApprenticeshipSearch(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ApprenticeshipSearchResult> Find<T>(T searchParams)
        {
            var handler =  _serviceProvider.GetService<IApprenticeshipSearchService<T>>();
            
            return await handler.Find(searchParams);
        }
    }
}
