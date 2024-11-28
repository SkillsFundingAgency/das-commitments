using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;

public class GetApprenticeshipsQueryHandler(
    IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails> mapper,
    IApprenticeshipSearch apprenticeshipSearch)
    : IRequestHandler<GetApprenticeshipsQuery, GetApprenticeshipsQueryResult>
{
    public async Task<GetApprenticeshipsQueryResult> Handle(GetApprenticeshipsQuery query, CancellationToken cancellationToken)
    {
        var matchedApprenticeshipDetails = new List<GetApprenticeshipsQueryResult.ApprenticeshipDetails>();

        ApprenticeshipSearchResult searchResult;

        if (string.IsNullOrEmpty(query.SortField))
        {
            var searchParameters = new ApprenticeshipSearchParameters
            {
                EmployerAccountId = query.EmployerAccountId,
                ProviderId = query.ProviderId,
                PageNumber = query.PageNumber,
                PageItemCount = query.PageItemCount,
                ReverseSort = query.ReverseSort,
                Filters = query.SearchFilters,
                CancellationToken = cancellationToken
            };

            searchResult = await apprenticeshipSearch.Find(searchParameters);
        }
        else
        {
            if (query.ReverseSort)
            {
                var searchParameters = new ReverseOrderedApprenticeshipSearchParameters
                {
                    EmployerAccountId = query.EmployerAccountId,
                    ProviderId = query.ProviderId,
                    PageNumber = query.PageNumber,
                    PageItemCount = query.PageItemCount,
                    ReverseSort = query.ReverseSort,
                    Filters = query.SearchFilters,
                    FieldName = query.SortField,
                    CancellationToken = cancellationToken
                };
                    
                searchResult = await apprenticeshipSearch.Find(searchParameters);
            }
            else
            {
                var searchParameters = new OrderedApprenticeshipSearchParameters
                {
                    EmployerAccountId = query.EmployerAccountId,
                    ProviderId = query.ProviderId,
                    PageNumber = query.PageNumber,
                    PageItemCount = query.PageItemCount,
                    ReverseSort = query.ReverseSort,
                    Filters = query.SearchFilters,
                    FieldName = query.SortField,
                    CancellationToken = cancellationToken
                };

                searchResult = await apprenticeshipSearch.Find(searchParameters);
            }
        }
        searchResult.Apprenticeships = searchResult.Apprenticeships
            .Select(c => { c.IsProviderSearch = query.ProviderId.HasValue; return c; })
            .ToList();

        foreach (var apprenticeship in searchResult.Apprenticeships)
        {
            var details = await mapper.Map(apprenticeship); 
            matchedApprenticeshipDetails.Add(details);
        }

        return new GetApprenticeshipsQueryResult
        {
            Apprenticeships = matchedApprenticeshipDetails,
            TotalApprenticeshipsFound = searchResult.TotalApprenticeshipsFound,
            TotalApprenticeshipsWithAlertsFound = searchResult.TotalApprenticeshipsWithAlertsFound,
            TotalApprenticeships = searchResult.TotalAvailableApprenticeships,
            PageNumber = searchResult.PageNumber
        };
    }
}