using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsHandler : IRequestHandler<GetApprenticeshipsRequest, GetApprenticeshipsResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;
        private readonly IMapper<Apprenticeship, ApprenticeshipDetails> _mapper;

        public GetApprenticeshipsHandler(
            IProviderCommitmentsDbContext dbContext,
            IMapper<Apprenticeship, ApprenticeshipDetails> mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetApprenticeshipsResponse> Handle(GetApprenticeshipsRequest request, CancellationToken cancellationToken)
        {
            var matchedApprenticeshipDetails = new List<ApprenticeshipDetails>();

            ApprenticeshipSearchResult searchResult;

            if (!string.IsNullOrEmpty(request.SortField) && request.SortField != nameof(Apprenticeship.DataLockStatus) && request.ReverseSort)
            {
                searchResult = await ApprenticeshipsReverseOrderedByField(cancellationToken, request.ProviderId, request.SortField, request.PageNumber, request.PageItemCount);
            }
            else if (!string.IsNullOrEmpty(request.SortField) && request.SortField != nameof(Apprenticeship.DataLockStatus))
            {
                searchResult = await ApprenticeshipsOrderedByField(cancellationToken, request.ProviderId, request.SortField, request.PageNumber, request.PageItemCount);
            }
            else if (string.IsNullOrEmpty(request.SortField) && request.ReverseSort)
            {
                searchResult = await ApprenticeshipsByReverseDefaultOrder(cancellationToken, request.ProviderId, request.PageNumber, request.PageItemCount);
            }
            else
            {
                searchResult = await ApprenticeshipsByDefaultOrder(cancellationToken, request.ProviderId, request.PageNumber, request.PageItemCount);
            }

            foreach (var apprenticeship in searchResult.Apprenticeships)
            {
                var details = await _mapper.Map(apprenticeship);
                matchedApprenticeshipDetails.Add(details);
            }

            return new GetApprenticeshipsResponse
            {
                Apprenticeships = matchedApprenticeshipDetails,
                TotalApprenticeshipsFound = searchResult.TotalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = searchResult.TotalApprenticeshipsWithAlertsFound
            };
        }

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsByDefaultOrder(CancellationToken cancellationToken, long? providerId, int pageNumber, int pageItemCount)
        {
            var apprenticeships = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId && apprenticeship.PendingUpdateOriginator != null)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .ToListAsync(cancellationToken);
            
            var apprenticeshipsWithoutAlerts = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId && apprenticeship.PendingUpdateOriginator == null)
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .ToListAsync(cancellationToken);

            var totalApprenticeshipsWithAlertsFound = apprenticeships.Count;

            apprenticeships.AddRange(apprenticeshipsWithoutAlerts);

            var totalApprenticeshipsFound = apprenticeships.Count;

            if (pageItemCount < 1 || pageNumber < 1)
            {
                apprenticeships = apprenticeships.ToList();
            }
            else
            {
                apprenticeships = apprenticeships.Skip((pageNumber - 1) * pageItemCount)
                    .Take(pageItemCount)
                    .ToList();
            }
            
            return new ApprenticeshipSearchResult
            {
                Apprenticeships = apprenticeships,
                TotalApprenticeshipsFound = totalApprenticeshipsFound,
                TotalApprenticeshipsWithAlertsFound = totalApprenticeshipsWithAlertsFound
            };
        }

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsByReverseDefaultOrder(CancellationToken cancellationToken, long? providerId,  int pageNumber, int pageItemCount)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId);

                var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(app => app.PendingUpdateOriginator != null, cancellationToken);

                apprenticeshipsQuery = apprenticeshipsQuery
                    .OrderByDescending(x => x.PendingUpdateOriginator != null)
                    .ThenBy(x => x.FirstName)
                    .ThenBy(x => x.LastName)
                    .ThenBy(x => x.Uln)
                    .ThenBy(x => x.Cohort.LegalEntityName)
                    .ThenBy(x => x.CourseName)
                    .ThenByDescending(x => x.StartDate)
                    .Include(apprenticeship => apprenticeship.Cohort)
                    .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(cancellationToken);

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

    
        private async Task<ApprenticeshipSearchResult>ApprenticeshipsOrderedByField(CancellationToken cancellationToken,long? providerId, string fieldName, int pageNumber, int pageItemCount)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(app => app.PendingUpdateOriginator != null, cancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderBy(GetOrderByField(fieldName))
                .ThenBy(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(cancellationToken);

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

        private async Task<ApprenticeshipSearchResult> ApprenticeshipsReverseOrderedByField(CancellationToken cancellationToken, long? providerId, string fieldName, int pageNumber, int pageItemCount)
        {
            var apprenticeshipsQuery = _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId);

            var totalApprenticeshipsWithAlertsFound = await apprenticeshipsQuery.CountAsync(app => app.PendingUpdateOriginator != null, cancellationToken);

            apprenticeshipsQuery = apprenticeshipsQuery
                .OrderByDescending(GetOrderByField(fieldName))
                .ThenByDescending(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus);

            var totalApprenticeshipsFound = await apprenticeshipsQuery.CountAsync(cancellationToken);

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
