using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryHandler : IRequestHandler<GetApprenticeshipsFilterValuesQuery, GetApprenticeshipsFilterValuesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprenticeshipsFilterValuesQueryHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprenticeshipsFilterValuesResponse> Handle(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
           var dbTasks = new []{
                GetDistinctEmployerNames(request, cancellationToken),
                GetDistinctCourseNames(request, cancellationToken),
                GetDistinctStatuses(request, cancellationToken),
                GetDistinctStartDates(request, cancellationToken),
                GetDistinctEndDates(request, cancellationToken)
            };

            Task.WaitAll(dbTasks.ToArray<Task>());
            
            return await Task.FromResult(new GetApprenticeshipsFilterValuesResponse
            {
                EmployerNames = dbTasks[0].Result,
                CourseNames = dbTasks[1].Result,
                Statuses = dbTasks[2].Result,
                PlannedStartDates = dbTasks[3].Result.Distinct(),
                PlannedEndDates = dbTasks[4].Result.Distinct()
            });
        }

        private Task<List<string>> GetDistinctEmployerNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctCourseNames(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctStatuses(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.Cohort.CommitmentStatus)
                .Distinct()
                .Select(status => Enum.GetName(typeof(CommitmentStatus), status))
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctStartDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.StartDate.HasValue ? apprenticeship.StartDate.Value.ToString("dd/MM/yyyy") : "N/A")
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctEndDates(GetApprenticeshipsFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Apprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.EndDate.HasValue ? apprenticeship.EndDate.Value.ToString("dd/MM/yyyy") : "N/A")
                .ToListAsync(cancellationToken);
        }
    }
}
