namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search
{
    public interface IApprenticeshipSearch
    {
        Task<ApprenticeshipSearchResult> Find<T>(T searchParams);
    }
}
