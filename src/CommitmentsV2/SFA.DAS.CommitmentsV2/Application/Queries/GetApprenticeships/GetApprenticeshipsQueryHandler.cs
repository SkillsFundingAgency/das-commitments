using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsQueryHandler : IRequestHandler<GetApprenticeshipsQuery, GetApprenticeshipsQueryResult>
    {
        private readonly ICommitmentsReadOnlyDbContext _dbContext;
        private readonly IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails> _mapper;

        public GetApprenticeshipsQueryHandler(
            ICommitmentsReadOnlyDbContext dbContext,
            IMapper<Apprenticeship, GetApprenticeshipsQueryResult.ApprenticeshipDetails> mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetApprenticeshipsQueryResult> Handle(GetApprenticeshipsQuery query, CancellationToken cancellationToken)
        {
            var matchedApprenticeshipDetails = new List<GetApprenticeshipsQueryResult.ApprenticeshipDetails>();

            ApprenticeshipSearchResult searchResult;

            if (string.IsNullOrEmpty(query.SortField) || query.SortField == "DataLockStatus")
            {
                searchResult = await ApprenticeshipsByDefaultOrder(cancellationToken, query.ProviderId, query.PageNumber, query.PageItemCount, query.ReverseSort, query.SearchFilters);
            }
            else
            {
                if (query.ReverseSort)
                {
                    searchResult = await ApprenticeshipsReverseOrderedByField(cancellationToken, query.ProviderId, query.SortField, query.PageNumber, query.PageItemCount, query.SearchFilters, query.PageNumber == 0);
                }
                else
                {
                    searchResult = await ApprenticeshipsOrderedByField(cancellationToken, query.ProviderId, query.SortField, query.PageNumber, query.PageItemCount, query.SearchFilters, query.PageNumber == 0);
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

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsByDefaultOrder(CancellationToken cancellationToken, long? providerId, int pageNumber, int pageItemCount, bool reverseSort, ApprenticeshipSearchFilters filters)
        {
            var totalApprenticeshipsWithoutAlerts = await GetApprenticeshipsQuery(providerId, filters, false, pageNumber == 0).CountAsync(cancellationToken);

            var totalApprenticeshipsWithAlerts = await GetApprenticeshipsQuery(providerId, filters, true, pageNumber == 0).CountAsync(cancellationToken);

            var skipCount = pageNumber > 0 ? (pageNumber - 1) * pageItemCount : 0;

            var apprentices = new List<Apprenticeship>();

            if (reverseSort)
            {
                if (skipCount < totalApprenticeshipsWithoutAlerts)
                {
                    var apprenticeshipsWithoutAlerts =
                        await GetApprenticeshipsWithoutAlerts(cancellationToken, providerId, skipCount, pageItemCount, true, filters, pageNumber);

                    apprentices.AddRange(apprenticeshipsWithoutAlerts);
                }

                if (pageItemCount != 0 && apprentices.Count == pageItemCount)
                {
                    return new ApprenticeshipSearchResult
                    {
                        Apprenticeships = apprentices,
                        TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                        TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts
                    };
                }

                skipCount = skipCount - totalApprenticeshipsWithoutAlerts > 0
                    ? skipCount - totalApprenticeshipsWithoutAlerts
                   : 0;

                pageItemCount = pageItemCount - apprentices.Count > 0 ? pageItemCount - apprentices.Count : 0;

                var apprenticeshipsWithAlerts = await GetApprenticeshipsWithAlerts(cancellationToken, providerId, skipCount, pageItemCount, true, filters, pageNumber);

                apprentices.AddRange(apprenticeshipsWithAlerts);
            }
            else
            {
                if (skipCount < totalApprenticeshipsWithAlerts)
                {
                    var apprenticeshipsWithAlerts =
                        await GetApprenticeshipsWithAlerts(cancellationToken, providerId, skipCount, pageItemCount, false, filters, pageNumber);

                    apprentices.AddRange(apprenticeshipsWithAlerts);
                }

                if (pageItemCount != 0 && apprentices.Count >= pageItemCount)
                {
                    return new ApprenticeshipSearchResult
                    {
                        Apprenticeships = apprentices,
                        TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                        TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts
                    };
                }

                skipCount = skipCount - totalApprenticeshipsWithAlerts > 0
                    ? skipCount - totalApprenticeshipsWithAlerts
                    : 0;

                pageItemCount = pageItemCount - apprentices.Count > 0 ? pageItemCount - apprentices.Count : 0;

                var apprenticeshipsWithoutAlerts = await GetApprenticeshipsWithoutAlerts(cancellationToken, providerId,  skipCount, pageItemCount, false, filters, pageNumber);

                apprentices.AddRange(apprenticeshipsWithoutAlerts);
            }

            return new ApprenticeshipSearchResult
            {
                Apprenticeships = apprentices,
                TotalApprenticeshipsFound = totalApprenticeshipsWithoutAlerts + totalApprenticeshipsWithAlerts,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlerts
            };
        }

        private async Task<List<Apprenticeship>> GetApprenticeshipsWithoutAlerts(CancellationToken cancellationToken, long? providerId, int skipCount, int pageItemCount, bool reverseOrder, ApprenticeshipSearchFilters filters, int pageNumber)
        {
            var query = GetApprenticeshipsQuery(providerId, filters, false, pageNumber == 0);

            query = query.OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort);

            if (skipCount > 0)
            {
                query = query.Skip(skipCount);
            }

            if (pageItemCount > 0)
            {
                query = query.Take(pageItemCount);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private async Task<List<Apprenticeship>> GetApprenticeshipsWithAlerts(CancellationToken cancellationToken, long? providerId, int skipCount, int pageItemCount, bool reverseOrder, ApprenticeshipSearchFilters filters, int pageNumber)
        {
            var query = GetApprenticeshipsQuery(providerId, filters, true, pageNumber == 0);

            query = query.OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate);

            if (skipCount > 0)
            {
                query = query.Skip(skipCount);
            }

            if (pageItemCount > 0)
            {
                query = query.Take(pageItemCount);
            }

            return await query.ToListAsync(cancellationToken);
        }

        private IQueryable<Apprenticeship> GetApprenticeshipsQuery(long? providerId, ApprenticeshipSearchFilters filters, bool withAlerts, bool isDownload)
        {
            return _dbContext
                .Apprenticeships
                .Where(withAlerts ? HasAlerts(providerId) : HasNoAlerts(providerId))
                .Filter(filters)
                .DownloadsFilter(isDownload);
        }

        private async Task<ApprenticeshipSearchResult>ApprenticeshipsOrderedByField(CancellationToken cancellationToken,long? providerId, string fieldName, int pageNumber, int pageItemCount, ApprenticeshipSearchFilters filters, bool isDownload)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .Filter(filters)
                .DownloadsFilter(isDownload);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(HasAlerts(providerId), cancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderBy(GetOrderByField(fieldName))
                .ThenBy(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(cancellationToken);

            return await CreatePagedApprenticeshipSearchResult(cancellationToken, pageNumber, pageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound);
        }

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsReverseOrderedByField(CancellationToken cancellationToken, long? providerId, string fieldName, int pageNumber, int pageItemCount, ApprenticeshipSearchFilters filters, bool isDownload)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .Filter(filters)
                .DownloadsFilter(isDownload);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(HasAlerts(providerId), cancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderByDescending(GetOrderByField(fieldName))
                .ThenByDescending(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(cancellationToken);

            return await CreatePagedApprenticeshipSearchResult(cancellationToken, pageNumber, pageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound);
        }
         
        private static async Task<ApprenticeshipSearchResult> CreatePagedApprenticeshipSearchResult(CancellationToken cancellationToken, int pageNumber,
            int pageItemCount, IQueryable<Apprenticeship> apprenticeshipsQuery, int totalApprenticeshipsFound,
            int totalApprenticeshipsWithAlertsFound)
        {
            List<Apprenticeship> apprenticeships;

            if (pageItemCount < 1 || pageNumber < 1)
            {
                apprenticeships = await apprenticeshipsQuery.ToListAsync(cancellationToken);
            }
            else
            {
                apprenticeships = await apprenticeshipsQuery.Skip((pageNumber - 1) * pageItemCount)
                    .Take(pageItemCount)
                    .ToListAsync(cancellationToken);
            }

            return new ApprenticeshipSearchResult
            {
                Apprenticeships = apprenticeships,
                TotalApprenticeshipsFound = totalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlertsFound
            };
        }

        private struct ApprenticeshipSearchResult
        {
            public IEnumerable<Apprenticeship> Apprenticeships { get; set; }
            public int TotalApprenticeshipsFound { get; set; }
            public int TotalApprenticeshipsWithAlertsFound { get; set; }
        }

        private static Expression<Func<Apprenticeship, bool>> HasAlerts(long? providerId)
        {
            return apprenticeship => apprenticeship.Cohort.ProviderId == providerId
                                     && (apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != EventStatus.Removed)
                                         || 
                                         apprenticeship.ApprenticeshipUpdate != null &&
                                         apprenticeship.ApprenticeshipUpdate.Any(
                                             c => c.Status == ApprenticeshipUpdateStatus.Pending 
                                                  && (c.Originator == Originator.Employer 
                                                      || c.Originator == Originator.Provider)
                                                  ));
        }

        private static Expression<Func<Apprenticeship, bool>> HasNoAlerts(long? providerId)
        {
            return apprenticeship => apprenticeship.Cohort.ProviderId == providerId
                                     && !apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != EventStatus.Removed)
                                     && apprenticeship.ApprenticeshipUpdate.All(c => c.Status != ApprenticeshipUpdateStatus.Pending);
        }

        private Expression<Func<Apprenticeship, object>> GetOrderByField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Apprenticeship.FirstName):
                    return apprenticeship => apprenticeship.FirstName;
                case nameof(Apprenticeship.LastName):
                    return apprenticeship => apprenticeship.LastName;
                case nameof(Apprenticeship.CourseName):
                    return apprenticeship => apprenticeship.CourseName;
                case nameof(Apprenticeship.Cohort.LegalEntityName):
                    return apprenticeship => apprenticeship.Cohort.LegalEntityName;
                case nameof(Apprenticeship.StartDate):
                    return apprenticeship => apprenticeship.StartDate;
                case nameof(Apprenticeship.EndDate):
                    return apprenticeship => apprenticeship.EndDate;
                case nameof(Apprenticeship.ApprenticeshipStatus):
                    return apprenticeship => apprenticeship.PaymentStatus;
                case nameof(Apprenticeship.Uln):
                    return apprenticeship => apprenticeship.Uln;
                default:
                    return apprenticeship => apprenticeship.FirstName;
            }
        }

        private Expression<Func<Apprenticeship, object>> GetSecondarySortByField(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Apprenticeship.FirstName):
                    return apprenticeship => apprenticeship.LastName;
                default:
                    return GetOrderByField(fieldName);
            }
        }
    }
}