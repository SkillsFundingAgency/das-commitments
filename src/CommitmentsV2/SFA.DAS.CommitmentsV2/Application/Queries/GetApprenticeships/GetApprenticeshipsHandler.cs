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

            var matched = await (string.IsNullOrEmpty(request.SortField) 
                    ? ApprenticeshipsByDefaultOrder(cancellationToken, request.ProviderId, request.PageNumber, request.PageItemCount) 
                    : ApprenticeshipsOrderedByField(cancellationToken,request.ProviderId, request.SortField, request.PageNumber, request.PageItemCount));

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

        private async Task<IEnumerable<Apprenticeship>> ApprenticeshipsByDefaultOrder(CancellationToken cancellationToken, long? providerId, int pageNumber, int pageItemCount)
        {
            var apprentices = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .OrderBy(x => x.PendingUpdateOriginator != null)
                .ThenBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ThenBy(x => x.Uln)
                .ThenBy(x => x.Cohort.LegalEntityName)
                .ThenBy(x => x.CourseName)
                .ThenByDescending(x => x.StartDate)
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Skip((pageNumber - 1) * pageItemCount)
                .Take(pageItemCount)
                .ToListAsync(cancellationToken);
           
            return apprentices;
        }

        private async Task<IEnumerable<Apprenticeship>> ApprenticeshipsOrderedByField(CancellationToken cancellationToken,long? providerId, string fieldName, int pageNumber, int pageItemCount)
        {
            var apprenticeships = await _dbContext
                .Apprenticeships
                .Where(apprenticeship => apprenticeship.Cohort.ProviderId == providerId)
                .OrderBy(x => x.DataLockStatus.Any(c => !c.IsResolved))
                .ThenBy(GetOrderByField(fieldName))
                .ThenBy(GetSecondarySortByField(fieldName))
                .Include(apprenticeship => apprenticeship.Cohort)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Include(apprenticeship => apprenticeship.DataLockStatus)
                .Skip((pageNumber - 1) * pageItemCount)
                .Take(pageItemCount)
                .ToListAsync(cancellationToken);
           
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
                case nameof(Apprenticeship.StopDate):
                    return apprenticeship => apprenticeship.StopDate;
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
