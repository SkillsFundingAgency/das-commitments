using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesQueryHandler : IRequestHandler<GetApprovedApprenticesFilterValuesQuery, GetApprovedApprenticesFilterValuesResponse>
    {
        private readonly IProviderCommitmentsDbContext _dbContext;

        public GetApprovedApprenticesFilterValuesQueryHandler(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetApprovedApprenticesFilterValuesResponse> Handle(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
           var dbTasks = new []{
                GetDistinctEmployerNames(request, cancellationToken),
                GetDistinctCourseNames(request, cancellationToken),
                GetDistinctStatuses(request, cancellationToken),
                GetDistinctStartDates(request, cancellationToken),
                GetDistinctEndDates(request, cancellationToken)
            };

            Task.WaitAll(dbTasks.ToArray<Task>());
            
            return await Task.FromResult(new GetApprovedApprenticesFilterValuesResponse
            {
                EmployerNames = dbTasks[0].Result,
                CourseNames = dbTasks[1].Result,
                Statuses = dbTasks[2].Result,
                PlannedStartDates = dbTasks[3].Result,
                PlannedEndDates = dbTasks[4].Result
            });
        }

        private Task<List<string>> GetDistinctEmployerNames(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.Cohort.LegalEntityName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctCourseNames(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.CourseName)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctStatuses(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.Cohort.CommitmentStatus)
                .Distinct()
                .Select(status => Enum.GetName(typeof(CommitmentStatus), status))
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctStartDates(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.StartDate.HasValue ? apprenticeship.StartDate.Value.ToString("dd/MM/yyyy") : "N/A")
                .Distinct()
                
                .ToListAsync(cancellationToken);
        }

        private Task<List<string>> GetDistinctEndDates(GetApprovedApprenticesFilterValuesQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.ApprovedApprenticeships
                .Include(apprenticeship => apprenticeship.Cohort)
                .Where(apprenticeship => apprenticeship.ProviderRef == request.ProviderId.ToString())
                .Select(apprenticeship => apprenticeship.EndDate.HasValue ? apprenticeship.EndDate.Value.ToString("dd/MM/yyyy") : "N/A")
                .Distinct()

                .ToListAsync(cancellationToken);
        }
    }
}
