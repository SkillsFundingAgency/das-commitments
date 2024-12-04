namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services;

public interface IApprenticeshipSearchService<in T>
{
    Task<ApprenticeshipSearchResult> Find(T searchParameters);
}