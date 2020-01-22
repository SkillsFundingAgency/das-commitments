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
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsHandler : IRequestHandler<GetApprenticeshipsRequest, GetApprenticeshipsResponse>
    {
        private readonly ICommitmentsReadOnlyDbContext _dbContext;
        private readonly IMapper<Apprenticeship, ApprenticeshipDetails> _mapper;

        public GetApprenticeshipsHandler(
            ICommitmentsReadOnlyDbContext dbContext,
            IMapper<Apprenticeship, ApprenticeshipDetails> mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetApprenticeshipsResponse> Handle(GetApprenticeshipsRequest request, CancellationToken cancellationToken)
        {
            var matchedApprenticeshipDetails = new List<ApprenticeshipDetails>();

            ApprenticeshipSearchResult searchResult;

            if (string.IsNullOrEmpty(request.SortField) || request.SortField == "DataLockStatus")
            {
                searchResult = await ApprenticeshipsByDefaultOrder(cancellationToken, request.ProviderId, request.PageNumber, request.PageItemCount, request.ReverseSort, request.SearchFilters);
            }
            else
            {
                if (request.ReverseSort)
                {
                    searchResult = await ApprenticeshipsReverseOrderedByField(cancellationToken, request.ProviderId, request.SortField, request.PageNumber, request.PageItemCount, request.SearchFilters);
                }
                else
                {
                    searchResult = await ApprenticeshipsOrderedByField(cancellationToken, request.ProviderId, request.SortField, request.PageNumber, request.PageItemCount, request.SearchFilters);
                }
            }

            foreach (var apprenticeship in searchResult.Apprenticeships)
            {
                var details = await _mapper.Map(apprenticeship);
                matchedApprenticeshipDetails.Add(details);
            }

            var totalAvailableApprenticeships = await _dbContext.Apprenticeships.CountAsync(apprenticeship => apprenticeship.Cohort.ProviderId == request.ProviderId, cancellationToken: cancellationToken);

            return new GetApprenticeshipsResponse
            {
                Apprenticeships = matchedApprenticeshipDetails,
                TotalApprenticeshipsFound = searchResult.TotalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = searchResult.TotalApprenticeshipsWithAlertsFound,
                TotalApprenticeships = totalAvailableApprenticeships
            };
        }

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsByDefaultOrder(CancellationToken cancellationToken, long? providerId, int pageNumber, int pageItemCount, bool reverseSort, ApprenticeshipSearchFilters filters)
        {
            var apprenticeships = await _dbContext
                .Apprenticeships
                .Where(HasAlerts(providerId))
                .Filter(filters)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.ApprenticeshipUpdate)
                .ToListAsync(cancellationToken);
            
            var apprenticeshipsWithoutAlerts = await _dbContext
                .Apprenticeships
                .Where(HasNoAlerts(providerId))
                .Filter(filters)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .ToListAsync(cancellationToken);

            var totalApprenticeshipsWithAlertsFound = apprenticeships.Count;

            List<Apprenticeship> combinedList;

            if (reverseSort)
            {
                combinedList = apprenticeshipsWithoutAlerts;
                combinedList.AddRange(apprenticeships);
            }
            else
            {
                combinedList = apprenticeships;
                combinedList.AddRange(apprenticeshipsWithoutAlerts);
            }

            var totalApprenticeshipsFound = combinedList.Count;

            if (pageItemCount < 1 || pageNumber < 1)
            {
                combinedList = combinedList.ToList();
            }
            else
            {
                combinedList = combinedList.Skip((pageNumber - 1) * pageItemCount)
                    .Take(pageItemCount)
                    .ToList();
            }

            return new ApprenticeshipSearchResult
            {
                Apprenticeships = combinedList,
                TotalApprenticeshipsFound = totalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlertsFound
            };
        }

        private async Task<ApprenticeshipSearchResult>ApprenticeshipsOrderedByField(CancellationToken cancellationToken,long? providerId, string fieldName, int pageNumber, int pageItemCount, ApprenticeshipSearchFilters filters)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .Filter(filters);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(HasAlerts(providerId), cancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderBy(GetOrderByField(fieldName))
                .ThenBy(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(cancellationToken);

            return await CreatePagedApprenticeshipSearchResult(cancellationToken, pageNumber, pageItemCount, apprenticeshipsQuery, totalApprenticeshipsFound, totalApprenticeshipsWithAlertsFound);
        }

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsReverseOrderedByField(CancellationToken cancellationToken, long? providerId, string fieldName, int pageNumber, int pageItemCount, ApprenticeshipSearchFilters filters)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .Filter(filters);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(HasAlerts(providerId), cancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderByDescending(GetOrderByField(fieldName))
                .ThenByDescending(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
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
                                     && (apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != 3)
                                         || 
                                         apprenticeship.ApprenticeshipUpdate != null &&
                                         apprenticeship.ApprenticeshipUpdate.Any(
                                             c => c.Status.Equals((byte)ApprenticeshipUpdateStatus.Pending) 
                                                  && (c.Originator.Equals((byte)Originator.Employer) 
                                                      || c.Originator.Equals((byte)Originator.Provider))));
        }

        private static Expression<Func<Apprenticeship, bool>> HasNoAlerts(long? providerId)
        {
            return apprenticeship => apprenticeship.Cohort.ProviderId == providerId
                                     && !apprenticeship.DataLockStatus.Any(c => !c.IsResolved && c.Status == Status.Fail && c.EventStatus != 3)
                                     && !apprenticeship.ApprenticeshipUpdate.Any(c => c.Status.Equals((byte)ApprenticeshipUpdateStatus.Pending));
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
                case nameof(Apprenticeship.PaymentStatus):
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
