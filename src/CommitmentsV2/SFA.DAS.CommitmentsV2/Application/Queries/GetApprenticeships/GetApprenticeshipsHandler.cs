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
            var mapped = new List<ApprenticeshipDetails>();

            var matched = new List<Apprenticeship>();

            if (!string.IsNullOrEmpty(request.SortField) && request.SortField != nameof(Apprenticeship.DataLockStatus) && request.ReverseSort)
            {
                matched = (List<Apprenticeship>) await ApprenticeshipsReverseOrderedByField(cancellationToken, request.ProviderId, request.SortField, request.IsDownload);
            }
            else if (!string.IsNullOrEmpty(request.SortField) && request.SortField != nameof(Apprenticeship.DataLockStatus))
            {
                matched = (List<Apprenticeship>) await ApprenticeshipsOrderedByField(cancellationToken, request.ProviderId, request.SortField, request.IsDownload);
            }
            else if (string.IsNullOrEmpty(request.SortField) && request.ReverseSort)
            {
                matched = (List<Apprenticeship>) await ApprenticeshipsByReverseDefaultOrder(cancellationToken, request.ProviderId, request.IsDownload);
            }
            else
            {
                matched = (List<Apprenticeship>) await ApprenticeshipsByDefaultOrder(cancellationToken, request.ProviderId, request.IsDownload);
            }

            foreach (var apprenticeship in matched)
            {
                var details = await _mapper.Map(apprenticeship);
                mapped.Add(details);
            }

            return new GetApprenticeshipsResponse
            {
                Apprenticeships = mapped
            };
        }

        private async Task<IEnumerable<Apprenticeship>> ApprenticeshipsByDefaultOrder(CancellationToken cancellationToken, long? providerId, bool isDownload)
        {
            var apprenticeshipsWithAlerts = await _dbContext
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

            apprenticeshipsWithAlerts.AddRange(apprenticeshipsWithoutAlerts);

            apprenticeshipsWithAlerts = FormatForDownload(apprenticeshipsWithAlerts, isDownload);

            return apprenticeshipsWithAlerts;
        }

        private async Task<IEnumerable<Apprenticeship>> ApprenticeshipsByReverseDefaultOrder(CancellationToken cancellationToken, long? providerId, bool isDownload)
        {
            var apprenticeships = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .OrderByDescending(x => x.PendingUpdateOriginator != null)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .ToListAsync(cancellationToken);

            apprenticeships = FormatForDownload(apprenticeships, isDownload);

            return apprenticeships;
        }

        private async Task<IEnumerable<Apprenticeship>> ApprenticeshipsOrderedByField(CancellationToken cancellationToken,long? providerId, string fieldName, bool isDownload)
        {
            var apprenticeships = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .OrderBy(GetOrderByField(fieldName))
                .ThenBy(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .ToListAsync(cancellationToken);

            apprenticeships = FormatForDownload(apprenticeships, isDownload);

            return apprenticeships;
        }

        private async Task<IEnumerable<Apprenticeship>> ApprenticeshipsReverseOrderedByField(CancellationToken cancellationToken, long? providerId, string fieldName, bool isDownload)
        {
            var apprenticeships = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .OrderByDescending(GetOrderByField(fieldName))
                .ThenByDescending(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .ToListAsync(cancellationToken);

            apprenticeships = FormatForDownload(apprenticeships, isDownload);

            return apprenticeships;
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

        private List<Apprenticeship> FormatForDownload(List<Apprenticeship> apprenticeships, bool isDownload)
        {
            if (isDownload)
            {
                var filteredApprenticeships = 
                    apprenticeships
                        .Where(x =>
                        x.EndDate >= DateTime.UtcNow.AddMonths(-12).Date &&
                        x.PaymentStatus == PaymentStatus.Completed)
                    .ToList();

                apprenticeships = filteredApprenticeships;
            }

            return apprenticeships;
        }
    }
}
