using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.DependencyResolution
{
    public class ApprenticeshipSearchRegistry : Registry
    {
        public ApprenticeshipSearchRegistry()
        {
            For<IApprenticeshipSearch>().Use<ApprenticeshipSearch>();
            For<IApprenticeshipSearchService<ApprenticeshipSearchParameters>>().Use<ApprenticeshipSearchService>();
            For<IApprenticeshipSearchService<OrderedApprenticeshipSearchParameters>>().Use<OrderedApprenticeshipSearchService>();
            For<IApprenticeshipSearchService<ReverseOrderedApprenticeshipSearchParameters>>().Use<ReverseOrderedApprenticeshipSearchService>();
        }
    }
}
