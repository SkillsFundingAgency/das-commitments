using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Handlers
{
    public interface IApprenticeshipSearchHandler<in T>
    {
        Task<ApprenticeshipSearchResult> Find(T searchParameters);
    }
}
