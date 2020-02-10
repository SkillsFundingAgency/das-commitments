using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Parameters;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsQueryHandler : IRequestHandler<GetApprenticeshipsQuery, GetApprenticeshipsQueryResult>
    {
        private readonly ICommitmentsReadOnlyDbContext _dbContext;
        private readonly IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails> _mapper;
        private readonly IApprenticeshipSearch _apprenticeshipSearch;

        public GetApprenticeshipsQueryHandler(
            ICommitmentsReadOnlyDbContext dbContext,
            IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails> mapper,
            IApprenticeshipSearch apprenticeshipSearch)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _apprenticeshipSearch = apprenticeshipSearch;
        }

        public async Task<GetApprenticeshipsQueryResult> Handle(GetApprenticeshipsQuery query, CancellationToken cancellationToken)
        {
            var matchedApprenticeshipDetails = new List<GetApprenticeshipsQueryResult.ApprenticeshipDetails>();

            ApprenticeshipSearchResult searchResult;

            if (string.IsNullOrEmpty(query.SortField) || query.SortField == "DataLockStatus")
            {
                var searchParameters = new ApprenticeshipSearchParameters
                {
                    ProviderId = query.ProviderId,
                    PageNumber = query.PageNumber,
                    PageItemCount = query.PageItemCount,
                    ReverseSort = query.ReverseSort,
                    Filters = query.SearchFilters,
                    CancellationToken = cancellationToken
                };

                searchResult = await _apprenticeshipSearch.Find(searchParameters);
            }
            else
            {
                if (query.ReverseSort)
                {
                    var searchParameters = new ReverseOrderedApprenticeshipSearchParameters
                    {
                        ProviderId = query.ProviderId,
                        PageNumber = query.PageNumber,
                        PageItemCount = query.PageItemCount,
                        ReverseSort = query.ReverseSort,
                        Filters = query.SearchFilters,
                        FieldName = query.SortField,
                        CancellationToken = cancellationToken
                    };
                    
                    searchResult = await _apprenticeshipSearch.Find(searchParameters);
                }
                else
                {
                    var searchParameters = new OrderedApprenticeshipSearchParameters
                    {
                        ProviderId = query.ProviderId,
                        PageNumber = query.PageNumber,
                        PageItemCount = query.PageItemCount,
                        ReverseSort = query.ReverseSort,
                        Filters = query.SearchFilters,
                        FieldName = query.SortField,
                        CancellationToken = cancellationToken
                    };

                    searchResult = await _apprenticeshipSearch.Find(searchParameters);
                }
            }

            foreach (var apprenticeship in searchResult.Apprenticeships)
            {
                var details = await _mapper.Map(apprenticeship);
                matchedApprenticeshipDetails.Add(details);
            }

            var totalAvailableApprenticeships = await _dbContext.Apprenticeships.CountAsync(apprenticeship => apprenticeship.Cohort.ProviderId == query.ProviderId, cancellationToken: cancellationToken);

            return new GetApprenticeshipsQueryResult
            {
                Apprenticeships = matchedApprenticeshipDetails,
                TotalApprenticeshipsFound = searchResult.TotalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = searchResult.TotalApprenticeshipsWithAlertsFound,
                TotalApprenticeships = totalAvailableApprenticeships
            };
        }
    }
}
